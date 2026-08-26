# [PY_RUNTIME_JOURNAL]

`Journal` owns the branch's durable evidence plane: one append-only stream of `AuditFact` and `MeterFact` rows draining through a bounded channel into whatever `Ledger` a composition binds, priced by exact-decimal rating and aged under one `Retain` vocabulary. Missing metric points read as dashboard gaps while a missing row is an evidence or billing defect, so appends suspend and retry without bound and every series projected beside them carries zero authority. Erasure destroys key material and never a row, so unreadable IS erased and the append-only plane survives whole.

The scope-bound `logger`, `Redaction`, and the `ScopeKey` axis arrive settled from `observability/observe#OBSERVE`, the `MEASURES` census from `observability/metrics#METRIC`, the point registry and install record from `observability/hooks#HOOKS`, the result and its fences from `reliability/faults#FAULT`, `Hlc` and `Tenant` from `evidence/clock#CLOCK`, `SecretBoundary` from `execution/admission#SETTINGS` as the one KEK reader the vault custody posture composes, and `ContentIdentity` from `evidence/identity#IDENTITY`. `Ledger` binds at the composition root that S0 never satisfies, stays async whole so no landing stalls the loop, and refuses unbound or structurally unmet with typed evidence.

## [01]-[INDEX]

- [02]-[FACT]: closed `Fact` family over its `Change` diff cases, the per-record projections every fold reads, the writer-owned stamp law with its wall-clock crossing, the `Retain` class table and its horizon, and the subject spine.
- [03]-[LEDGER]: `Ledger` port, content-keyed row projection, never-shedding bounded channel with its lossless close and unlanded roster, the three-state intake and async-boundary laws every producer binds against, and the groom fold.
- [04]-[RATING]: associative rollup monoid over a totally-ordered price key and the exact-decimal charge fold under one terminal quantize.
- [05]-[SHREDDER]: `SubjectKey` custody identity, AAD-bound envelope algebra over one native crossing, total open, portability export, and erasure as key destruction.

## [02]-[FACT]

- Owner: two msgspec-tagged records close the `Fact` family — `AuditFact` carries actor, action, target, the closed `Change` diff over JSON-pointer paths, and the subjects it touched; `MeterFact` carries an integral quantity against the `Resource` vocabulary — each holding its own disjoint field set, so the wire tag IS the family discriminant and no type column stands beside a payload bag. Each record ALSO carries its own `stream`, `retention`, `gate`, `kind`, `subjects`, and `measures` projection, so the ledger, the drain, the gate, and the series read one shape and no consuming fold re-derives the discriminant at a site that can drift from its siblings. `Series` closes this owner's measure vocabulary and `install` proves the whole of it against the census, so units live at their one owner and a second spelling here cannot diverge.
- Cases: `Change` splits by which sides a diff carries — `Assigned` the arrival, `Cleared` the departure, `Shifted` both — so a policy fold types every evidence shape and a free-form details bag never enters. `Party` splits by which vocabulary its kind axis closes on — the actor half on `Actor`, refused at decode outside that roster, the target half open because a target names whatever noun its verb touched — so one shape serves both positions and neither carries a second class column to disagree with. `Resource` transcribes the cross-branch metering roster whole; a further resource lands as one `RESOURCES` row beside its rate row in BOTH branch spellings — this one and `libs/typescript/data/.planning/journal/fact.md` `_RESOURCES` — since a runtime-local addition forks a vocabulary the peer prices against. Metric-egress tenancy diverges from that peer BY DESIGN: the drain projects OUTSIDE any producer's context and the budgeted attribute fold `observability/metrics#METRIC` owns resolves tenancy from baggage, so this branch's journal series carry no tenant dimension at all and tenancy resolves on the row, where the peer prices the tag per resource on its own row; the fact row carries tenancy identically on both, so only the lossy metric projection differs.
- Entry: `Journal.record` is the one stamp authority — it merges the inbound `CausalFrame` half a caller threads, joins this process's own physical sample, ticks the successor under the gate, and REPLACES the slot on every fact it admits. Caller-threaded stamps make identity a discipline rather than a construction, since the content key covers the stamped payload and two producers reusing one coordinate collapse two genuine facts onto one key. Wall time never orders the stream: rows sort on `Hlc.packed`, which survives a ledger carrying no identity column and reconstructs order across processes that shared no sequence.
- Entry: `at` is the one crossing from wall clock into that coordinate and refuses a naive instant, so a billing window a human settles on and a groom cutoff both derive in the unit the stream already sorts on; every tick conversion runs integer arithmetic end to end, so a settlement boundary lands exact and no float mantissa rounds a stamp. `horizon` never touches wall clock at all — it subtracts each finite window in ticks from this owner's own sample, so cutoff and stamp share one time base by construction.
- Auto: retention is constitution for one stream and policy for the other — a `MeterFact` is `REGULATORY` because it is billing truth, an `AuditFact` names the class its own policy demands — so the groom horizon reads one column and no consumer re-derives a stream's class.
- Auto: tenancy resolves at the WRITER — `record` fills a `None` slot from the one `TENANT_BAGGAGE` entry the `observability/metrics#METRIC` attribute fold already keys on, so ambient tenancy reaches the durable row through one reader rather than five producers each spelling the same lookup, and a fact arriving WITH tenancy keeps it verbatim, since a producer recording on behalf of one tenant from inside another's context is exactly what a re-read overwrites. Absence past that fold is genuine: the wire omits it under `omit_defaults` and the domain reads absence as single-tenant, so an unattributed fact records absence and never forges a tenancy nobody held. The metric tenant BUDGET never crosses over with the key — it bounds a series value axis, and this plane carries no cardinality ceiling to bound.
- Auto: `AuditFact.subjects` IS the portability index, carried on the fact and projected onto its row, so the export scan and the erasure key on ONE composite and the spine law holds by construction rather than through a companion table a ledger writes inside the same landing. Facts naming no subject index nothing and stay invisible to every subject read.
- Law: hook ids close on this folder's own `JournalGate` roster and every refusal resolves ONE `reliability/faults#FAULT` `RAISES` anchor under `RuntimeLeg.JOURNAL`, so no bare string constructs a point row or spells a fence coordinate. `JOURNAL_UNBOUND` is ONE parameterized row over both unbound boundaries, the verb its NAMED slot, since a bind read and a close read refuse the same law.
- Growth: a new evidence kind is one `Change` case with its arm in every consuming fold; a new actor class is one `Actor` member in both branch spellings; a new metered resource is one `Resource` member with its `RESOURCES` row and its rate row; a new fact stream is one more tagged record carrying the family's projections, which the row fold, the gate, the drain, and the groom inherit unedited; a new retention class is one `Retain` member with its `WINDOWS` row; a new derived series is one `Series` member the census admits; a new diff grammar is one `Pointer` pattern edit; a newly classified field is one `FACT_REDACTION` row. A `Resource` member also widens what `Rating` completeness means at the next settlement — `rated` refuses by name on a resource its caller-supplied rating omits — so the rate row lands with the member or the first window carrying it settles nothing.
- Boundary: this family carries the retention KEY and never the window — `WINDOWS` prices the class and the ledger executes the reclaim, so no page outside this owner spells a duration. Quantities stay integral by constraint, which is what keeps the exact-decimal crossing in `[04]` free of any float: a fractional need is a smaller unit row, never a decimal quantity.

```python
# --- [IMPORTS] --------------------------------------------------------------------------
from collections.abc import AsyncIterator, Awaitable, Callable, Iterable, Mapping
from datetime import UTC, datetime, timedelta
from decimal import ROUND_HALF_EVEN, Context, Decimal, DecimalException, DivisionByZero, FloatOperation, InvalidOperation, Overflow
from enum import StrEnum
from inspect import iscoroutinefunction
from random import uniform
from secrets import token_bytes
from threading import RLock
from time import time_ns
from typing import Annotated, ClassVar, Final, Literal, Protocol, Self, get_args, get_protocol_members

import anyio
import anyio.to_thread
from anyio import TASK_STATUS_IGNORED, BrokenResourceError, CapacityLimiter, ClosedResourceError
from anyio.abc import TaskStatus
from anyio.streams.memory import MemoryObjectReceiveStream, MemoryObjectSendStream
from expression import Error, Nothing, Ok, Option, Result, Some, case, tag, tagged_union
from expression.collections import Block, Map
from expression.extra.result import traverse
from msgspec import Meta, Struct, to_builtins
from msgspec.json import schema_components
from msgspec.msgpack import Decoder, Encoder
from msgspec.structs import replace
from opentelemetry import baggage
from opentelemetry import context as otel_context

from rasm.runtime.admission import SecretBoundary, SecretShape
from rasm.runtime.clock import CausalFrame, Hlc, Tenant
from rasm.runtime.faults import (
    JOURNAL_APPEND,
    JOURNAL_CENSUS,
    JOURNAL_CHARGE,
    JOURNAL_CRYPTO,
    JOURNAL_CUSTODY,
    JOURNAL_DERIVED,
    JOURNAL_HEX,
    JOURNAL_INSTANT,
    JOURNAL_KEK,
    JOURNAL_OFFER,
    JOURNAL_PERIOD,
    JOURNAL_PORT,
    JOURNAL_RATE,
    JOURNAL_RETIRED,
    JOURNAL_UNBOUND,
    JOURNAL_UNDRAINED,
    SCOPES,
    Catch,
    Disposition,
    FaultRow,
    RuntimeLeg,
    RuntimeResult,
    Scope,
    async_boundary,
    boundary,
    traversed,
)
from rasm.runtime.hooks import HookId, HookPoint, Hooks, Modality
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.metrics import MEASURES, TENANT_BAGGAGE, Metrics
from rasm.runtime.observe import DEFAULT_SCOPE, REDACTED, REDACTION_KEY, Redaction, ScopeKey, logger

lazy from cryptography.exceptions import InvalidTag, UnsupportedAlgorithm
lazy from cryptography.hazmat.primitives.ciphers.aead import AESGCMSIV, AESSIV

# --- [TYPES] ----------------------------------------------------------------------------

type Pointer = Annotated[str, Meta(pattern=r"\A(/[^/~]*(~[01][^/~]*)*)*\Z")]
type Verb = Annotated[str, Meta(pattern=r"\A[a-z][a-z0-9]*(\.[a-z][a-z0-9]*)+\Z")]
type Quantity = Annotated[int, Meta(ge=0)]
type Subject = Annotated[str, Meta(min_length=1, max_length=200)]
type Stream = Literal["audit", "meter"]


class Actor(StrEnum):
    USER = "user"
    SERVICE = "service"
    SYSTEM = "system"


class Resource(StrEnum):
    COMPUTE = "compute"
    RECORD = "record"
    REQUEST = "request"
    STORAGE = "storage"
    TOKEN = "token"


class Retain(StrEnum):
    EPHEMERAL = "ephemeral"
    OPERATIONAL = "operational"
    REGULATORY = "regulatory"
    PERMANENT = "permanent"


class Series(StrEnum):
    APPENDED = "rasm.journal.appended"
    DEDUPED = "rasm.journal.deduped"
    DEFERRED = "rasm.journal.deferred"
    ERASED = "rasm.journal.erased"
    GROOMED = "rasm.journal.groomed"
    DURATION = "rasm.journal.metered.duration"
    TALLY = "rasm.journal.metered.tally"
    VOLUME = "rasm.journal.metered.volume"


class Motion(StrEnum):
    APPEND = "append"
    GROOM = "groom"
    ERASE = "erase"


# --- [CONSTANTS] ------------------------------------------------------------------------

OWNER: Final[str] = "journal"

NANOS_PER_TICK: Final[int] = 100
TICKS_PER_MICRO: Final[int] = 1_000 // NANOS_PER_TICK
EPOCH: Final[datetime] = datetime(1970, 1, 1, tzinfo=UTC)
MICRO: Final[timedelta] = timedelta(microseconds=1)

AUDIT_STREAM: Final[Stream] = "audit"
METER_STREAM: Final[Stream] = "meter"
STREAMS: Final[tuple[Stream, ...]] = get_args(Stream.__value__)

UNSTAMPED: Final[Hlc] = Hlc(physical_ticks=0, logical=0)

INTAKE: Final[int] = 512
BATCH_WIDTH: Final[int] = 128
FLUSH_SECONDS: Final[float] = 2.0

BACKOFF_SECONDS: Final[float] = 0.1
BACKOFF_CEILING: Final[float] = 10.0

NONCE_BYTES: Final[int] = 12
KEY_BITS: Final[int] = 256
KEK_BITS: Final[int] = 512
AEAD_SLOTS: Final[CapacityLimiter] = CapacityLimiter(8)

MONEY: Final[Context] = Context(
    prec=34, rounding=ROUND_HALF_EVEN, traps=[InvalidOperation, DivisionByZero, Overflow, FloatOperation]
)
SCALE: Final[Decimal] = Decimal("0.0001")

class JournalGate(StrEnum):
    APPEND = "rasm.runtime.journal.append"
    ERASE = "rasm.runtime.journal.erase"

# --- [MODELS] ---------------------------------------------------------------------------


class Assigned(Struct, tag="assigned", frozen=True, gc=False):
    path: Pointer
    next: str


class Cleared(Struct, tag="cleared", frozen=True, gc=False):
    path: Pointer
    prior: str


class Shifted(Struct, tag="shifted", frozen=True, gc=False):
    path: Pointer
    prior: str
    next: str


type Change = Assigned | Cleared | Shifted


class Party[K: str](Struct, frozen=True, gc=False):
    kind: K
    key: str
    parent: str | None = None


class AuditFact(Struct, tag=AUDIT_STREAM, frozen=True, omit_defaults=True):
    stream: ClassVar[Stream] = AUDIT_STREAM
    gate: ClassVar[HookId | None] = JournalGate.APPEND
    resource: ClassVar[Resource | None] = None
    quantity: ClassVar[Quantity] = 0

    action: Verb
    actor: Party[Actor]
    target: Party[str]
    retention: Retain
    change: tuple[Change, ...] = ()
    subjects: tuple[Subject, ...] = ()
    tenant: Tenant | None = None
    stamp: Hlc = UNSTAMPED

    @property
    def kind(self) -> str:
        return self.retention.value

    @property
    def measures(self) -> Mapping[str, float]:
        return {Series.APPENDED: 1.0}


class MeterFact(Struct, tag=METER_STREAM, frozen=True, omit_defaults=True):
    stream: ClassVar[Stream] = METER_STREAM
    gate: ClassVar[HookId | None] = None
    retention: ClassVar[Retain] = Retain.REGULATORY
    subjects: ClassVar[tuple[Subject, ...]] = ()

    resource: Resource
    quantity: Quantity
    surface: str | None = None
    tenant: Tenant | None = None
    stamp: Hlc = UNSTAMPED

    @property
    def kind(self) -> str:
        return self.resource.value

    @property
    def measures(self) -> Mapping[str, float]:
        return {Series.APPENDED: 1.0, RESOURCES[self.resource]: float(self.quantity)}


type Fact = AuditFact | MeterFact
type Recordable = Fact | Iterable[Fact]

ENCODE: Final[Callable[[Fact], bytes]] = Encoder(order="deterministic").encode
DECODE: Final[Callable[[bytes], Fact]] = Decoder(type=Fact).decode


class FactRow(Struct, frozen=True):
    key: ContentKey
    stamp: Hlc
    stream: Stream
    service: str
    retention: Retain
    payload: bytes
    subjects: tuple[Subject, ...] = ()
    tenant: Tenant | None = None
    resource: Resource | None = None
    quantity: Quantity = 0


SCHEMA: Final[tuple[tuple[dict[str, object], ...], dict[str, object]]] = schema_components((Fact, FactRow))


class Landing(Struct, frozen=True, gc=False):
    accepted: Block[ContentKey]
    duplicate: Block[ContentKey]


class JournalDrain(Struct, frozen=True, gc=False):
    landed: int = 0
    deduped: int = 0

    @staticmethod
    def combined(left: "JournalDrain", right: "JournalDrain") -> "JournalDrain":
        return JournalDrain(landed=left.landed + right.landed, deduped=left.deduped + right.deduped)


class Groomed(Struct, frozen=True, gc=False):
    reclaimed: int


class Tombstone(Struct, frozen=True, gc=False):
    subject: Subject
    tenant: Tenant
    destroyed: Hlc


class Sealed(Struct, frozen=True, gc=False):
    nonce: bytes
    cipher: bytes


class SubjectKey(Struct, frozen=True, gc=False):
    tenant: Tenant
    subject: Subject

    @property
    def aad(self) -> bytes:
        return f"rasm.subject:{self.tenant}:{self.subject}".encode()


class Custody(Struct, frozen=True):
    wrap: Callable[[bytes, bytes], Awaitable[RuntimeResult[bytes]]]
    unwrap: Callable[[bytes, bytes], Awaitable[RuntimeResult[bytes]]]

    @classmethod
    def local(cls, kek: bytes) -> Self:
        async def wrapped(material: bytes, aad: bytes) -> RuntimeResult[bytes]:
            return _crypto("journal.wrap", lambda: AESSIV(kek).encrypt(material, [aad]))

        async def unwrapped(held: bytes, aad: bytes) -> RuntimeResult[bytes]:
            return _crypto("journal.unwrap", lambda: AESSIV(kek).decrypt(held, [aad]))

        return cls(wrap=wrapped, unwrap=unwrapped)

    @classmethod
    def vault(cls, boundary: SecretBoundary, service: str) -> Self:
        async def material() -> RuntimeResult[bytes]:
            resolved = await boundary.resolve(service, shape=SecretShape.TOKEN)
            return resolved.bind(
                lambda held: held.map(lambda secret: _kek(secret.get_secret_value())).default_with(
                    lambda: Error(JOURNAL_CUSTODY.raised(service))
                )
            )

        async def wrapped(payload: bytes, aad: bytes) -> RuntimeResult[bytes]:
            return (await material()).bind(lambda kek: _crypto("journal.wrap", lambda: AESSIV(kek).encrypt(payload, [aad])))

        async def unwrapped(held: bytes, aad: bytes) -> RuntimeResult[bytes]:
            return (await material()).bind(lambda kek: _crypto("journal.unwrap", lambda: AESSIV(kek).decrypt(held, [aad])))

        return cls(wrap=wrapped, unwrap=unwrapped)


class Period(Struct, frozen=True, gc=False):
    stream: Stream
    since: Hlc
    until: Hlc
    tenant: Tenant | None = None

    @classmethod
    def of(cls, stream: Stream, since: datetime, until: datetime, tenant: Tenant | None = None) -> RuntimeResult[Self]:
        return at(since).map2(at(until), lambda lower, upper: (lower, upper)).bind(
            lambda bounds: Ok(cls(stream=stream, since=bounds[0], until=bounds[1], tenant=tenant))
            if bounds[0].packed <= bounds[1].packed
            else Error(JOURNAL_PERIOD.raised())
        )


@tagged_union(frozen=True)
class Scan:
    tag: Literal["period", "subject"] = tag()
    period: Period = case()
    subject: SubjectKey = case()


class Bound(Struct, frozen=True):
    ledger: "Ledger"
    custody: Custody
    service: str


class JournalInstall(Struct, frozen=True):
    ledger: str
    service: str
    streams: tuple[Stream, ...]
    classes: tuple[Retain, ...]


# --- [TABLES] ---------------------------------------------------------------------------

RESOURCES: Final[Map[Resource, Series]] = Map.of_seq([
    (Resource.COMPUTE, Series.DURATION),
    (Resource.RECORD, Series.TALLY),
    (Resource.REQUEST, Series.TALLY),
    (Resource.STORAGE, Series.VOLUME),
    (Resource.TOKEN, Series.TALLY),
])

WINDOWS: Final[Map[Retain, Option[timedelta]]] = Map.of_seq([
    (Retain.EPHEMERAL, Some(timedelta(days=7))),
    (Retain.OPERATIONAL, Some(timedelta(days=90))),
    (Retain.REGULATORY, Some(timedelta(days=2555))),
    (Retain.PERMANENT, Nothing),
])

POINTS: Final[Block[HookPoint[Struct]]] = Block.of_seq([
    HookPoint(id=JournalGate.APPEND, payload=AuditFact, modality=Modality(veto=None)),
    HookPoint(id=JournalGate.ERASE, payload=Tombstone, modality=Modality(observe=None)),
])

FACT_REDACTION: Final[Redaction] = Redaction(
    classified=Map.of_seq([("prior", "mask"), ("next", "mask"), ("key", "hash"), ("subjects", "hash")])
)

# --- [OPERATIONS] -----------------------------------------------------------------------


def _sampled() -> Hlc:
    return Hlc(physical_ticks=time_ns() // NANOS_PER_TICK, logical=0)


def _ticks(span: timedelta) -> int:
    return span // MICRO * TICKS_PER_MICRO


def at(moment: datetime) -> RuntimeResult[Hlc]:
    return (
        Ok(Hlc(physical_ticks=(moment - EPOCH) // MICRO * TICKS_PER_MICRO, logical=0))
        if moment.utcoffset() is not None
        else Error(JOURNAL_INSTANT.raised())
    )


def horizon(now: Hlc) -> Map[Retain, Hlc]:
    return Map.of_seq(
        (clazz, Hlc(physical_ticks=max(now.physical_ticks - _ticks(window), 0), logical=0))
        for clazz, held in WINDOWS.items()
        for window in held.to_list()
    )


def _kek(text: str) -> RuntimeResult[bytes]:
    return boundary(JOURNAL_HEX, lambda: bytes.fromhex(text), catch=ValueError).bind(
        lambda kek: Ok(kek) if len(kek) * 8 == KEK_BITS else Error(JOURNAL_KEK.raised(str(len(kek) * 8), str(KEK_BITS)))
    )


def _crypto[T](axis: str, run: Callable[[], T]) -> RuntimeResult[T]:
    return boundary(JOURNAL_CRYPTO, lambda: (InvalidTag, UnsupportedAlgorithm, ValueError, OverflowError), catch=ImportError).bind(
        lambda catch: boundary(JOURNAL_CRYPTO, run, catch=catch).map_error(lambda fault: JOURNAL_CRYPTO.raised(axis, fault.detail))
    )


async def _offloaded[T](axis: str, run: Callable[[], T]) -> RuntimeResult[T]:
    lifted = await async_boundary(JOURNAL_CRYPTO, lambda: anyio.to_thread.run_sync(lambda: _crypto(axis, run), limiter=AEAD_SLOTS), catch=OSError)
    return lifted.bind(lambda inner: inner)


def _censused() -> RuntimeResult[Block[Series]]:
    rows = Block.of_seq(Series)
    unmet = rows.filter(lambda row: MEASURES.try_find((OWNER, row.value)).is_none())
    return Ok(rows) if unmet.is_empty() else Error(JOURNAL_CENSUS.raised(", ".join(unmet)))
```

## [03]-[LEDGER]

- Owner: `Ledger` is the one durable port and `Journal` the one writer; the drain TAKES the receive handle out of custody, so single consumership is structural — a second concurrent drain on one scope refuses instead of splitting the stream — and the stamp cell, the batch fold, and the retry cadence have exactly one reader. `_rowed` is the one projection into the ledger — content key, stamp, stream token, service, class, subject index, metering pair, and opaque msgpack payload — so the durable schema is a value this page owns, `SCHEMA` hands the implementer that contract as machine-readable data, no consumer re-derives a column, and the projection is TOTAL, since a result there opens a shed path under the one law this plane refuses.
- Owner: every coordinate a ledger PREDICATES, GROUPS, or PARTITIONS on lifts into its own row column and only the fact body stays opaque — subject index, retention class, stamp window, and the `(resource, quantity)` metering pair alike — so `tallied` is a pushed-down group-by rather than a decode hop wearing that name, and `Aggregate.rolled` publishes the same algebra for an engine whose reader carries no grouped form. Withholding a column a port member needs turns that member into the full-window materialization the delegation exists to avoid.
- Owner: `Landing.accepted` NAMES the rows the plane did not already hold and `duplicate` names the redeliveries the content key matched, both as keys, because the drain's line, its series, and its metered quantities fire off the accepted half ALONE — the append retry never exhausts, so a batch replayed after a lost acknowledgement costs one absorbed duplicate rather than a doubled charge and a doubled audit line, and a half reported as a bare tally names no row that filter can read. One fused count across both arms satisfies the completeness sum while claiming zero redelivery, and zero redelivery is indistinguishable from a wedged retry re-offering one window forever; `rasm.journal.deduped` carries that half off the drain, so an inflated `accepted` deletes the one signal proving at-least-once delivery is happening.
- Cases: every port member awaits, because a ledger writes durably and the drain runs on the loop — a synchronous member stalls the scheduler for the whole batch, and the branch admits no on-loop blocking arm; the install proof refuses a member present yet not a coroutine function, which otherwise faults inside the retry indistinguishable from a dead ledger. `_chained` is the one async bind carrying a settled result into an awaited continuation, `_resolved` composing it over the bound carrier, so an unbound scope refuses once and a custody-and-ledger chain short-circuits with no hand-repeated match.
- Law: `install` proves the census and the port structurally, registers the point rows, and binds only then — a measure or a member missing refuses at the bind naming it, where an unchecked bind defers the failure into the unbounded retry and reads as a dead ledger forever. Re-entry returns the standing `JournalInstall`, so a second composition-root pass never swaps a ledger out from under a live drain; point registration latches per scope, because ids are composition-unique and the registry ships no retirement. `install` deposits that record on the `observability/hooks#HOOKS` install ledger, so a support bundle answers which durable plane a composition wrote to, under which vocabulary, and as which service, with no second custody surface.
- Law: `closed` retires custody with the intake and MARKS the scope retired, so a composition that shuts down re-arms by installing again rather than adopting a closed stream every `record` then faults `resource` against, and the mark is what lets `record` separate a plane that died from one that never existed — `_pointed` cannot, latching past every retirement by design. Unlanded rows survive that retirement, since a shutdown owing them is exactly when a caller reads them.
- Law: `record` resolves THREE intake states and never two — an armed scope sends, a scope whose custody `closed` RETIRED refuses with the `config` fault naming the port, and a scope no composition ever installed folds to `Ok(0)`. That third state is a deployment DECLARATION that this process journals nothing: its facts drop lawfully, so every producer binds the same path into its verdict whatever the deployment armed, a refusal on an armed plane still surfaces, and an unarmed one costs one map read. The unarmed fold records no series either — a drop counter prices the hot path of a composition that asked for no plane, and the install census proving this owner's measures never ran on that scope. Collapsing the pair into one refusal fails every producer's verdict wherever evidence was never deployed; collapsing it the other way renders retired custody — a producer outliving its own plane, which is a real fault — as lawful silence.
- Law: a producer records at the nearest ASYNC fold that OWNS the fact, and no synchronous leg records — recording suspends by law, so a sync spelling can only shed exactly what the never-shed result refuses to shed, while the metric projection of the same fact records synchronously at the producing site. An owner whose whole surface is synchronous mints its awaitable leg over the band hop its callers hand-roll rather than moving the record onto the loop-blocking side. A producer's `action` spells `<domain>.<operation>` — the same `domain` segment its metric projection carries beside the operation its own dispatch names — so one verb greps against the series its evidence twin emitted and no central verb registry stands between them.
- Entry: `record` folds arity off the value so a lone fact and a batch of either stream take one entry, and stamps every fact it admits; an EMPTY offer is admitted arity rather than a refusal — a metering fan that priced nothing holds no row to charge, which is what a zero-quantity producer leg settles to — so it fires no gate, ticks no stamp, and answers the count it landed, while a RETIRED scope refuses it exactly as it refuses a full batch, since that fault names a producer outliving its plane and no batch width makes that lawful; `drained` is the composition-root coroutine a task group starts — `tg.start` blocks on its readiness signal, so no producer suspends into an intake nothing reads, and the root reads the terminal tally off the child handle — and `closed` is its lossless counterpart. Recording is async by law — the send suspends under back-pressure, and no synchronous spelling can suspend — so a sync producer re-enters the loop through the portal bridge exactly as every other foreign-thread crossing does. `drained` and `closed` refuse an unbound scope with a `config` fault naming the port; `record` splits that absence under the three-state law above.
- Auto: the ledger lands the batch FIRST and its `Landing` must PARTITION the offered keys — disjoint halves whose union is the whole set — so a short write, an overlapping half, and a foreign key all retry rather than projecting; only an exact landing projects, only its ACCEPTED half does, and an observer never reads a fact the durable plane refused or already held. Retry attempts never exhaust and the decorrelated-jitter delay caps, so a dead ledger costs a bounded cadence a fleet never synchronizes on, while the bounded intake propagates pressure back through the suspended writer; the deferral counter reports the ledger and the drain's occupancy probe reports intake depth with suspended senders, so a full intake behind a healthy ledger is visible pressure too.
- Auto: shutdown closes the intake and awaits the drain, never cancels it — `anyio` delivers every buffered fact after the last send end closes, so the partial window flushes, `drained` returns its tally, and nothing in flight sheds. Roots that must nevertheless bound the wait wrap the await in their own `CancelScope` and read `pending` after it trips: a tripped scope returns no value, so the drain's terminal deposits BOTH the batch it was retrying and the checkpoint-free sweep of whatever still sat in the buffer, and every fact either landed or is named. Deadline parameters here instead re-thread the cancellation a scope already owns and cap the steady-state retry the never-shed law forbids capping.
- Law: `_pending` ACCUMULATES and settles by key — `_owed` appends and `_settled` removes exactly the keys a landing covered, so a scope re-installed after `closed` still owes what its prior drain never landed. Replacing the slot hands the next session's first batch that debt to overwrite, and a blanket clear erases it on the first success, both shedding evidence on the one plane whose whole thesis is that nothing sheds.
- Law: three fences hold a catch-all and each states why — a derived write runs a caller's render, record, and sink, an append calls a caller-supplied `Ledger` implementer, and both must never fault the plane owning the truth; every other fence names its provider set.
- Growth: a new durable coordinate is one `FactRow` column reaching the ledger and the row projection; a new read shape is one `Scan` case with its ledger arm; a new drain posture is one flow or backoff constant; a new ledger family is one implementer of the port with zero edits here.
- Boundary: a `Ledger` implementer's own landing path records NOTHING — `landed` reaches durability through whatever commit surface that implementer composes, so a producer leg seated on that surface re-enters `record` for every batch it lands and the stream feeds itself without bound. The implementer's composed owners therefore discriminate the ledger's OWN relations from a caller's and record only the caller's, and the durable plane's emptiness cannot do it for them: a journal commit is indistinguishable from a caller's commit by store alone, so the discriminant is the relation identity the ledger's tables declare at open.
- Boundary: this owner opens no connection, mints no statement, and names no engine — the ledger executes every landing, scan, tally, and reclaim through its own mechanism, so retention, compaction, and rollup ride machinery a ledger already carries and no worker or scheduler surface enters this branch for telemetry. Append gating admits or refuses and never transforms: the veto fold's returned payload is discarded by law, because a subscriber rewriting evidence makes the plane it observes a second author.

```python
# --- [OPERATIONS] -----------------------------------------------------------------------


def _rowed(fact: Fact, service: str) -> FactRow:
    payload = ENCODE(fact)
    return FactRow(
        key=ContentIdentity.key(OWNER, payload),
        stamp=fact.stamp,
        stream=fact.stream,
        service=service,
        retention=fact.retention,
        payload=payload,
        subjects=fact.subjects,
        tenant=fact.tenant,
        resource=fact.resource,
        quantity=fact.quantity,
    )


def _fenced(at: FaultRow[RuntimeLeg], run: Callable[[], object], scope: ScopeKey) -> None:
    boundary(at, run, catch=Exception).swap().map(lambda fault: logger(scope).warning(at.subject, **fault.facts()))


def _series(measures: Mapping[str, float], kind: str, scope: ScopeKey) -> None:
    _fenced(JOURNAL_DERIVED, lambda: Metrics.record(measures, domain=OWNER, kind=kind, scope=scope), scope)


def _projected(fact: Fact, scope: ScopeKey) -> None:
    _series(fact.measures, fact.kind, scope)
    line = logger(scope).bind(**{REDACTION_KEY: FACT_REDACTION})
    _fenced(JOURNAL_DERIVED, lambda: line.info(fact.kind, stream=fact.stream, **to_builtins(fact, str_keys=True)), scope)


async def _batched(receive: MemoryObjectReceiveStream[Fact]) -> AsyncIterator[Block[Fact]]:
    async for head in receive:
        held = [head]
        with anyio.move_on_after(FLUSH_SECONDS):
            while len(held) < BATCH_WIDTH:
                try:
                    held.append(await receive.receive())
                except anyio.EndOfStream:
                    break
        yield Block.of_seq(held)


def _swept(receive: MemoryObjectReceiveStream[Fact], service: str) -> Block[FactRow]:
    held: list[FactRow] = []
    while True:
        try:
            held.append(_rowed(receive.receive_nowait(), service))
        except (anyio.WouldBlock, anyio.EndOfStream):
            return Block.of_seq(held)


async def _chained[T, U](held: RuntimeResult[T], step: Callable[[T], Awaitable[RuntimeResult[U]]]) -> RuntimeResult[U]:
    match held:
        case Result(tag="ok", ok=value):
            return await step(value)
        case refused:
            return Error(refused.error)


async def _resolved[T](scope: ScopeKey, run: Callable[["Bound"], Awaitable[RuntimeResult[T]]]) -> RuntimeResult[T]:
    return await _chained(Journal.bound(scope), run)


def _proven(ledger: object) -> RuntimeResult["Ledger"]:
    unmet = Block.of_seq(sorted(member for member in get_protocol_members(Ledger) if not iscoroutinefunction(getattr(ledger, member, None))))
    return Ok(ledger) if unmet.is_empty() else Error(JOURNAL_PORT.raised(", ".join(unmet)))


def _partitions(landing: Landing, rows: Block[FactRow]) -> bool:
    offered = frozenset(row.key for row in rows)
    admitted, matched = frozenset(landing.accepted), frozenset(landing.duplicate)
    return (
        admitted.isdisjoint(matched)
        and admitted | matched == offered
        and len(landing.accepted) + len(landing.duplicate) == len(rows)
    )


def _tenanted(fact: Fact) -> Fact:
    match baggage.get_baggage(TENANT_BAGGAGE, otel_context.get_current()) if fact.tenant is None else None:
        case str() as tenant if tenant:
            return replace(fact, tenant=Tenant(tenant))
        case _:
            return fact


def _reclaimed(swept: Groomed, scope: ScopeKey) -> Groomed:
    _series({Series.GROOMED: float(swept.reclaimed)}, Motion.GROOM, scope)
    return swept


# --- [SERVICES] -------------------------------------------------------------------------


class Ledger(Protocol):
    async def landed(self, rows: Block[FactRow], /) -> RuntimeResult[Landing]: ...
    async def scanned(self, scan: Scan, /) -> RuntimeResult[Block[Fact]]: ...
    async def tallied(self, scan: Scan, /) -> RuntimeResult["Billed"]: ...
    async def groomed(self, horizon: Map[Retain, Hlc], /) -> RuntimeResult[Groomed]: ...
    async def claimed(self, subject: SubjectKey, wrapped: bytes, /) -> RuntimeResult[bytes]: ...
    async def held(self, subject: SubjectKey, /) -> RuntimeResult[Option[bytes]]: ...
    async def destroyed(self, stone: Tombstone, /) -> RuntimeResult[Option[Tombstone]]: ...


class Journal:
    _bound: ClassVar[Map[ScopeKey, Bound]] = Map.empty()
    _intake: ClassVar[Map[ScopeKey, MemoryObjectSendStream[Fact]]] = Map.empty()
    _drain: ClassVar[Map[ScopeKey, MemoryObjectReceiveStream[Fact]]] = Map.empty()
    _pending: ClassVar[Map[ScopeKey, Block[FactRow]]] = Map.empty()
    _installs: ClassVar[Map[ScopeKey, JournalInstall]] = Map.empty()
    _pointed: ClassVar[frozenset[ScopeKey]] = frozenset()
    _retired: ClassVar[frozenset[ScopeKey]] = frozenset()
    _stamp: ClassVar[Hlc] = UNSTAMPED
    _installing: ClassVar[Map[ScopeKey, RLock]] = Map.empty()
    _gate = RLock()

    @classmethod
    def stamped(cls, causal: Option[CausalFrame] = Nothing) -> Hlc:
        observed = causal.map(lambda frame: frame.hlc).default_value(cls._stamp)
        with cls._gate:
            cls._stamp = Hlc.merge(cls._stamp, observed).tick(_sampled())
            return cls._stamp

    @classmethod
    def install(
        cls, ledger: Ledger, custody: Custody, *, service: str = SCOPES[Scope.JOURNAL], scope: ScopeKey = DEFAULT_SCOPE
    ) -> RuntimeResult[JournalInstall]:
        with cls._installing_lock(scope):
            with cls._gate:
                standing = cls._installs.try_find(scope)
            match standing:
                case Option(tag="some", some=prior):
                    return Ok(prior)
                case _:
                    return (
                        _censused()
                        .bind(lambda _rows: _proven(ledger))
                        .bind(lambda _port: cls._registered(scope))
                        .map(lambda _points: cls._bind(ledger, custody, service, scope))
                    )

    @classmethod
    def _installing_lock(cls, scope: ScopeKey) -> RLock:
        with cls._gate:
            held = cls._installing.try_find(scope).default_with(RLock)
            cls._installing = cls._installing.add(scope, held)
        return held

    @classmethod
    def _registered(cls, scope: ScopeKey) -> RuntimeResult[Block[HookPoint[Struct]]]:
        return (
            Ok(Block.empty()) if scope in cls._pointed else Hooks.register(POINTS, scope=scope).map(lambda points: cls._latched(scope, points))
        )

    @classmethod
    def _latched(cls, scope: ScopeKey, points: Block[HookPoint[Struct]]) -> Block[HookPoint[Struct]]:
        with cls._gate:
            cls._pointed = cls._pointed | {scope}
        return points

    @classmethod
    def _bind(cls, ledger: Ledger, custody: Custody, service: str, scope: ScopeKey) -> JournalInstall:
        send, receive = anyio.create_memory_object_stream[Fact](max_buffer_size=INTAKE)
        bound = JournalInstall(ledger=type(ledger).__qualname__, service=service, streams=STREAMS, classes=tuple(Retain))
        with cls._gate:
            cls._bound = cls._bound.add(scope, Bound(ledger=ledger, custody=custody, service=service))
            cls._intake, cls._drain = cls._intake.add(scope, send), cls._drain.add(scope, receive)
            cls._installs = cls._installs.add(scope, bound)
            cls._retired = cls._retired - {scope}
        return Hooks.installed(OWNER, bound, scope=scope)

    @classmethod
    def bound(cls, scope: ScopeKey = DEFAULT_SCOPE) -> RuntimeResult[Bound]:
        return cls._bound.try_find(scope).to_result_with(lambda: JOURNAL_UNBOUND.raised("bound"))

    @classmethod
    async def record(
        cls, source: Recordable, *, causal: Option[CausalFrame] = Nothing, scope: ScopeKey = DEFAULT_SCOPE
    ) -> RuntimeResult[int]:
        offered = Block.of_seq((source,) if isinstance(source, AuditFact | MeterFact) else source)
        match cls._intake.try_find(scope):
            case Option(tag="some", some=send):
                attributed = Block.of_seq(_tenanted(fact) for fact in offered)
                match cls._admitted(attributed, scope):
                    case Result(tag="ok"):
                        return await cls._offered(send, attributed.map(lambda fact: replace(fact, stamp=cls.stamped(causal))))
                    case refused:
                        return Error(refused.error)
            case _ if scope in cls._retired:
                return Error(JOURNAL_RETIRED.raised())
            case _:
                return Ok(0)

    @staticmethod
    def _admitted(facts: Block[Fact], scope: ScopeKey) -> RuntimeResult[Block[Fact]]:
        return traversed(
            facts.choose(lambda fact: Option.of_optional(fact.gate).map(lambda point: Hooks.fire(point, fact, scope=scope))),
            by=Disposition.ABORT,
        )

    @staticmethod
    async def _offered(send: MemoryObjectSendStream[Fact], facts: Block[Fact]) -> RuntimeResult[int]:
        async def suspended() -> int:
            for fact in facts:
                await send.send(fact)
            return len(facts)

        return await async_boundary(JOURNAL_OFFER, suspended, catch=(BrokenResourceError, ClosedResourceError))

    @classmethod
    async def drained(
        cls, *, scope: ScopeKey = DEFAULT_SCOPE, task_status: TaskStatus[None] = TASK_STATUS_IGNORED
    ) -> RuntimeResult[JournalDrain]:
        with cls._gate:
            taken = cls._drain.try_find(scope)
            cls._drain = cls._drain.remove(scope)
        match (taken, cls.bound(scope)):
            case (Option(tag="some", some=receive), Result(tag="ok", ok=held)):
                settled = JournalDrain()
                task_status.started()
                async with receive:
                    with Metrics.occupied(
                        lambda: (depth := receive.statistics()).current_buffer_used + depth.tasks_waiting_send,
                        band=OWNER,
                        scope=scope,
                    ):
                        try:
                            async for batch in _batched(receive):
                                settled = JournalDrain.combined(settled, await cls._landed(held, batch, scope))
                        finally:
                            cls._owed(scope, _swept(receive, held.service))
                return Ok(settled)
            case _:
                return Error(JOURNAL_UNDRAINED.raised())

    @classmethod
    def closed(cls, *, scope: ScopeKey = DEFAULT_SCOPE) -> RuntimeResult[None]:
        with cls._gate:
            held, taken = cls._intake.try_find(scope), cls._drain.try_find(scope)
            service = cls._bound.try_find(scope).map(lambda bound: bound.service)
            cls._intake, cls._drain = cls._intake.remove(scope), cls._drain.remove(scope)
            cls._bound, cls._installs = cls._bound.remove(scope), cls._installs.remove(scope)
            cls._retired = cls._retired | {scope}
        taken.bind(lambda receive: service.map(lambda name: cls._stranded(scope, receive, name)))
        return held.to_result_with(lambda: JOURNAL_UNBOUND.raised("close")).map(lambda send: send.close())

    @classmethod
    def _stranded(cls, scope: ScopeKey, receive: MemoryObjectReceiveStream[Fact], service: str) -> None:
        cls._owed(scope, _swept(receive, service))
        receive.close()

    @classmethod
    def pending(cls, *, scope: ScopeKey = DEFAULT_SCOPE) -> Block[FactRow]:
        with cls._gate:
            return cls._pending.try_find(scope).default_value(Block.empty())

    @classmethod
    def _owed(cls, scope: ScopeKey, rows: Block[FactRow]) -> None:
        with cls._gate:
            cls._pending = cls._pending.add(scope, cls._pending.try_find(scope).default_value(Block.empty()).append(rows))

    @classmethod
    def _settled(cls, scope: ScopeKey, rows: Block[FactRow]) -> None:
        landed = frozenset(row.key for row in rows)
        with cls._gate:
            cls._pending = cls._pending.add(
                scope, cls._pending.try_find(scope).default_value(Block.empty()).filter(lambda row: row.key not in landed)
            )

    @classmethod
    async def _landed(cls, held: Bound, batch: Block[Fact], scope: ScopeKey) -> JournalDrain:
        paired = batch.map(lambda fact: (fact, _rowed(fact, held.service)))
        rows = paired.map(lambda pair: pair[1])
        cls._owed(scope, rows)
        delay = BACKOFF_SECONDS
        while True:
            match await async_boundary(JOURNAL_APPEND, lambda: held.ledger.landed(rows), catch=Exception):
                case Result(tag="ok", ok=Result(tag="ok", ok=landing)) if _partitions(landing, rows):
                    admitted = frozenset(landing.accepted)
                    cls._settled(scope, rows)
                    _series({Series.DEDUPED: float(len(landing.duplicate))}, Motion.APPEND, scope)
                    paired.filter(lambda pair: pair[1].key in admitted).fold(
                        lambda _held, pair: _projected(pair[0], scope), None
                    )
                    return JournalDrain(landed=len(landing.accepted), deduped=len(landing.duplicate))
                case _:
                    _series({Series.DEFERRED: 1.0}, Motion.APPEND, scope)
                    await anyio.sleep(delay)
                    delay = min(uniform(BACKOFF_SECONDS, delay * 3.0), BACKOFF_CEILING)

    @classmethod
    async def groomed(cls, *, scope: ScopeKey = DEFAULT_SCOPE) -> RuntimeResult[Groomed]:
        async def swept(held: Bound) -> RuntimeResult[Groomed]:
            return (await held.ledger.groomed(horizon(_sampled()))).map(lambda done: _reclaimed(done, scope))

        return await _resolved(scope, swept)
```

## [04]-[RATING]

- Owner: `Aggregate` carries the whole rollup algebra — `combined` the component-wise additive monoid over the `ZERO` identity, `rolled` the fold keying meter facts by `Priced` — associative by construction, so window rollups fuse across drains and a settled period composes from window tallies rather than one re-read of every row. `rolled` is PUBLISHED rather than private because the ledger's own `tallied` is its second caller: an engine carrying no grouped read folds this algebra over its scan instead of re-deriving a billing rollup beside a durable plane, where the two spellings drift on the first `Priced` column. `rated` prices those rows against a caller-supplied `Rating`.
- Cases: `Priced` carries a TOTAL order because `expression.Map` is an ordered tree — the tenancy half ranks on its own `attributed` column and holds `""` only underneath it, so a mixed-tenancy window folds, where a bare `Tenant | None` key raises comparing `None` against a tenant string on the second distinct tenancy. `held` is the one reader turning that pair back into absence, so tenancy never becomes a sentinel outside the fold key.
- Entry: the module `rolled` discriminates on the value — a block of facts in hand folds through `Aggregate.rolled`, a `Scan` coordinate delegates the group-by to the bound ledger, whose engine groups the `(tenant, resource)` columns `FactRow` lifts and sums `quantity` in place — so one entry serves an in-process tally and a settled billing period, and no consumer mints a second query. `Period.of` is how a caller reaches that coordinate from the wall-clock instants a settlement names.
- Auto: rates are caller-supplied policy because prices are application law and never library constants, so a resource the `Rating` omits refuses by name rather than defaulting a charge the application never authored; a rate decoded from settings preserves its scale exactly, since `msgspec` round-trips a `Decimal` field as a JSON string instead of through a float.
- Growth: a new charge model — tiered, floored, minimum-billed — is one field on the `Rate` row read inside `rated`, never a second rating entry; a new aggregate moment is one `Aggregate` field its monoid folds; a new billing dimension is one `Priced` column its order already spans.
- Boundary: exact arithmetic is absolute — the integral total lifts losslessly, multiplication runs inside the money context whose armed `FloatOperation` trap makes the no-float law structural rather than disciplinary, and the single terminal quantize is the only rounding in the path. This law stands stricter than the sibling projection at `python:data/tabular/cost#COST`, which prices approximate resource spend for a dashboard; a charge a customer settles admits no float at any step.

```python
# --- [MODELS] ---------------------------------------------------------------------------


class Rate(Struct, frozen=True, gc=False):
    per: Decimal
    currency: str


class Aggregate(Struct, frozen=True, gc=False):
    count: int
    total: int

    @staticmethod
    def combined(left: "Aggregate", right: "Aggregate") -> "Aggregate":
        return Aggregate(count=left.count + right.count, total=left.total + right.total)

    @staticmethod
    def rolled(facts: "Block[Fact]") -> "Billed":
        def keyed(held: "Billed", fact: "MeterFact") -> "Billed":
            return held.change(
                Priced.of(fact),
                lambda standing: Some(Aggregate.combined(standing.default_value(ZERO), Aggregate(count=1, total=fact.quantity))),
            )

        return facts.choose(lambda fact: Some(fact) if isinstance(fact, MeterFact) else Nothing).fold(keyed, Map.empty())


class Charge(Struct, frozen=True, gc=False):
    amount: Decimal
    currency: str


class Priced(Struct, frozen=True, order=True, gc=False):
    attributed: bool
    tenant: str
    resource: Resource

    @classmethod
    def of(cls, fact: MeterFact) -> Self:
        return cls(attributed=fact.tenant is not None, tenant=fact.tenant or "", resource=fact.resource)

    @property
    def held(self) -> Option[Tenant]:
        return Some(Tenant(self.tenant)) if self.attributed else Nothing


type Billed = Map[Priced, Aggregate]
type Rating = Map[Resource, Rate]

ZERO: Final[Aggregate] = Aggregate(count=0, total=0)

# --- [OPERATIONS] -----------------------------------------------------------------------


async def rolled(source: Block[Fact] | Scan, *, scope: ScopeKey = DEFAULT_SCOPE) -> RuntimeResult[Billed]:
    match source:
        case Scan() as coordinate:

            async def tallied(held: Bound) -> RuntimeResult[Billed]:
                return await held.ledger.tallied(coordinate)

            return await _resolved(scope, tallied)
        case facts:
            return Ok(Aggregate.rolled(facts))


def _priced(key: Priced, aggregate: Aggregate, rating: Rating) -> RuntimeResult[tuple[Priced, Charge]]:
    return rating.try_find(key.resource).to_result_with(lambda: JOURNAL_RATE.raised(key.resource.value)).bind(
        lambda rate: boundary(
            JOURNAL_CHARGE,
            lambda: (key, Charge(amount=MONEY.quantize(MONEY.multiply(Decimal(aggregate.total), rate.per), SCALE), currency=rate.currency)),
            catch=DecimalException,
        )
    )


def rated(billed: Billed, rating: Rating) -> RuntimeResult[Map[Priced, Charge]]:
    return traverse(lambda row: _priced(row[0], row[1], rating), Block.of_seq(billed.items())).map(Map.of_seq)
```

## [05]-[SHREDDER]

- Owner: `SubjectKey` is the tenant-scoped custody identity, the ledger holds one wrapped data key per identity, and `sealed`/`opened`/`erased` compose the envelope algebra over it. Custody stores the WRAPPED form alone — raw data keys never cross the ledger boundary — so a posture changes by swapping a `Custody` value and this page never learns which holder issued the wrap; the custody arms are async whole exactly as the ledger is, so a remote KMS arm binds as an ordinary instance rather than an on-loop stall, and both wrap under deterministic AAD-bound AES-SIV, so the KEK path carries no nonce custody to misuse. Two instances ship: `local` holds material a root already resolved, and `vault` reads the deployment's own credential ladder through the settings-admitted secret boundary on EVERY call, so a rotated KEK reaches the next wrap with no rebind and an unnamed one refuses instead of minting a substitute that would leave every prior envelope permanently unopenable.
- Cases: `opened` is TOTAL over erasure — a destroyed or absent key folds to `Nothing`, which every reader renders through the observe-owned redaction marker, because erasure is a lawful state and never an error to recover from. `InvalidTag` on LIVE key material stays a fault on the result: tampering and erasure are different facts, and folding a tag failure to absence renders a corrupted payload as a lawfully erased one.
- Entry: `sealed` claims atomically — a fresh mint inserts, a concurrent or replayed subject keeps the stored wrapped key and the loser seals under the winner by unwrapping the returned row, and a destroyed subject resurrects under a NEW key so every envelope written before the erasure stays unreadable forever. Claiming before sealing is load-bearing: two recorders racing one subject otherwise seal under two data keys, and destroying either leaves half that subject's evidence readable.
- Entry: `exported` is the portability read — one `Scan.subject` over the same index every append wrote and every erasure keys on, so a data-subject request is an index scan rather than a stream crawl. Sealed fields inside a payload stay sealed: field shapes are application material, so the exporting consumer composes `opened`/`redacted` per field it knows, and an erased subject's fields render the marker rather than failing the export.
- Auto: every envelope binds to its `SubjectKey` through the associated-data slot, so a ciphertext lifted onto another subject or another tenant fails its authentication tag rather than opening under a key that happens to be live. Every AEAD call crosses one native fence, so an unbuildable `cryptography` classifies `import_` on the result wherever the process first touches it — a read-only replica that never seals included.
- Auto: `erased` mints the tombstone under the writer's own stamp — a ledger supplies neither order nor identity, so it only empties the custody slot and echoes the stone it persisted — records it as a regulatory audit fact carrying the erased subject in its own index, and fans the observe point through the async mirror so an async compliance observer is reachable. Destruction is irreversible, so that record's result BINDS into the erasure verdict: a refused or unlanded tombstone surfaces naming the subject whose key is already gone, where a dropped result leaves an erasure no export can evidence.
- Growth: a new custody posture is one `Custody` instance the composition root binds beside the shipped `local` and `vault` pair — a hardware holder or a cloud KMS arm lands as one more value, never a field on either; a new secret backend behind `vault` is a `CloudVault` arm at the settings owner with zero edit here; a new sealed field is a caller-side projection composing `sealed`/`opened`, since field shapes are application material; a new export surface is one projection over the same `Export`, never a second subject read.
- Boundary: erasure destroys key material and touches no row — the append-only invariant survives the right to erasure because unreadable IS erased — and the export and the erasure prove one spine: export reads what remains readable, erasure makes fields unreadable, and the stored bytes stay untouched either way.

```python
# --- [MODELS] ---------------------------------------------------------------------------


class Export(Struct, frozen=True):
    subject: SubjectKey
    facts: Block[Fact]


# --- [OPERATIONS] -----------------------------------------------------------------------


async def _cipher(data_key: bytes, plain: bytes, key: SubjectKey) -> RuntimeResult[Sealed]:
    nonce = token_bytes(NONCE_BYTES)
    return await _offloaded("journal.seal", lambda: Sealed(nonce=nonce, cipher=AESGCMSIV(data_key).encrypt(nonce, plain, key.aad)))


async def sealed(plain: bytes, key: SubjectKey, *, scope: ScopeKey = DEFAULT_SCOPE) -> RuntimeResult[Sealed]:
    async def claimed(held: Bound) -> RuntimeResult[Sealed]:
        offered = await _chained(
            _crypto("journal.mint", lambda: AESGCMSIV.generate_key(bit_length=KEY_BITS)), lambda fresh: held.custody.wrap(fresh, key.aad)
        )
        winner = await _chained(offered, lambda material: held.ledger.claimed(key, material))
        data_key = await _chained(winner, lambda material: held.custody.unwrap(material, key.aad))
        return await _chained(data_key, lambda opened_key: _cipher(opened_key, plain, key))

    return await _resolved(scope, claimed)


async def opened(envelope: Sealed, key: SubjectKey, *, scope: ScopeKey = DEFAULT_SCOPE) -> RuntimeResult[Option[bytes]]:
    async def unsealed(held: Bound) -> RuntimeResult[Option[bytes]]:
        match await held.ledger.held(key):
            case Result(tag="ok", ok=Option(tag="some", some=material)):
                data_key = await held.custody.unwrap(material, key.aad)
                plain = await _chained(
                    data_key,
                    lambda opened_key: _offloaded("journal.open", lambda: AESGCMSIV(opened_key).decrypt(envelope.nonce, envelope.cipher, key.aad)),
                )
                return plain.map(Some)
            case Result(tag="ok", ok=Option(tag="none")):
                return Ok(Nothing)
            case refused:
                return Error(refused.error)

    return await _resolved(scope, unsealed)


async def exported(key: SubjectKey, *, scope: ScopeKey = DEFAULT_SCOPE) -> RuntimeResult[Export]:
    async def scanned(held: Bound) -> RuntimeResult[Export]:
        return (await held.ledger.scanned(Scan(subject=key))).map(lambda facts: Export(subject=key, facts=facts))

    return await _resolved(scope, scanned)


def redacted(held: Option[bytes]) -> bytes:
    return held.default_value(REDACTED.encode())


def _erasure(stone: Tombstone) -> AuditFact:
    return AuditFact(
        action="subject.erased",
        actor=Party(kind=Actor.SYSTEM, key="retain"),
        target=Party(kind="subject", key=stone.subject),
        retention=Retain.REGULATORY,
        change=(Cleared(path="/key", prior="wrapped"),),
        subjects=(stone.subject,),
        tenant=stone.tenant,
    )


async def erased(key: SubjectKey, *, scope: ScopeKey = DEFAULT_SCOPE) -> RuntimeResult[Option[Tombstone]]:
    async def destroyed(held: Bound) -> RuntimeResult[Option[Tombstone]]:
        stone = Tombstone(subject=key.subject, tenant=key.tenant, destroyed=Journal.stamped())
        match await held.ledger.destroyed(stone):
            case Result(tag="ok", ok=Option(tag="some", some=persisted)):
                await Hooks.fire_async(JournalGate.ERASE, persisted, scope=scope)
                _series({Series.ERASED: 1.0}, Motion.ERASE, scope)
                return (await Journal.record(_erasure(persisted), scope=scope)).map(lambda _landed: Some(persisted))
            case settled:
                return settled

    return await _resolved(scope, destroyed)
```

## [06]-[RESEARCH]

<!-- source-only: research row template; every landed row opens on the list dash this placeholder omits, the census reading `^- [TOKEN]-[STATUS]:` alone:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)

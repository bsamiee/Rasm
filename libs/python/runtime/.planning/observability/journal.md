# [PY_RUNTIME_JOURNAL]

`Journal` owns the branch's durable evidence plane: one append-only stream of `AuditFact` and `MeterFact` rows draining through a bounded rail into whatever `Ledger` a composition binds, priced by exact-decimal rating and aged under one `Retain` vocabulary. Missing metric points read as dashboard gaps while a missing row is an evidence or billing defect, so appends suspend and retry without bound and every series projected beside them carries zero authority. Erasure destroys key material and never a row, so unreadable IS erased and the append-only plane survives whole.

Receipt emission, redaction, and the `ScopeKey` axis arrive settled from `observability/receipts#RECEIPT`, the `MEASURES` census from `observability/metrics#METRIC`, the point registry and install record from `observability/hooks#HOOKS`, the rail and its fences from `reliability/faults#FAULT`, `Hlc` and `Tenant` from `evidence/clock#CLOCK`, `SecretBoundary` from `execution/admission#SETTINGS` as the one KEK reader the vault custody posture composes, and `ContentIdentity` from `evidence/identity#IDENTITY`. `Ledger` binds at the composition root that S0 never satisfies, stays async whole so no landing stalls the loop, and refuses unbound or structurally unmet with typed evidence.

## [01]-[INDEX]

- [02]-[FACT]: closed `Fact` family over its `Change` diff cases, the per-record projections every fold reads, the writer-owned stamp law with its wall-clock crossing, the `Retain` class table and its horizon, and the subject spine.
- [03]-[LEDGER]: `Ledger` port, content-keyed row projection, never-shedding bounded rail with its lossless close and unlanded roster, the three-state intake and async-seam laws every producer binds against, and the groom fold.
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
- Growth: a new evidence kind is one `Change` case with its arm in every consuming fold; a new actor class is one `Actor` member in both branch spellings; a new metered resource is one `Resource` member with its `RESOURCES` row and its rate row; a new fact stream is one more tagged record carrying the family's projections, which the row fold, the gate, the drain, and the groom inherit unedited; a new retention class is one `Retain` member with its `WINDOWS` row; a new derived series is one `Series` member the census admits; a new diff grammar is one `Pointer` pattern edit; a newly classified field is one `FACT_REDACTION` row. A `Resource` member also widens what `Rating` completeness means at the next settlement — `rated` refuses by name on a resource its caller-supplied rating omits — so the rate row lands with the member or the first window carrying it settles nothing.
- Boundary: this family carries the retention KEY and never the window — `WINDOWS` prices the class and the ledger executes the reclaim, so no page outside this owner spells a duration. Quantities stay integral by constraint, which is what keeps the exact-decimal crossing in `[04]` free of any float: a fractional need is a smaller unit row, never a decimal quantity.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
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
from anyio import TASK_STATUS_IGNORED, CapacityLimiter
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
from rasm.runtime.faults import SCOPES, BoundaryFault, Disposition, RuntimeRail, Scope, async_boundary, boundary, traversed
from rasm.runtime.hooks import HookPoint, Hooks, Modality
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.metrics import MEASURES, TENANT_BAGGAGE, Metrics
from rasm.runtime.receipts import DEFAULT_SCOPE, OPEN, REDACTED, Receipt, Redaction, ScopeKey, Signals

lazy from cryptography.exceptions import InvalidTag, UnsupportedAlgorithm  # cold native tier: reified at the first crossing
lazy from cryptography.hazmat.primitives.ciphers.aead import AESGCMSIV, AESSIV

# --- [TYPES] ----------------------------------------------------------------------------

# JSON-pointer path and dotted audit verb, both refined at the field so the diff vocabulary greps and groups without
# a central verb registry; `Meta` runs inside the C decoder, so a malformed historical row refuses at admission
# rather than reaching a policy fold that cannot type it. Both anchor on `\A`/`\Z` because `Meta` matches by SEARCH
# and `$` admits a trailing newline — a verb or pointer carrying one otherwise lands as a distinct grep token that
# renders identically to its clean twin.
type Pointer = Annotated[str, Meta(pattern=r"\A(/[^/~]*(~[01][^/~]*)*)*\Z")]
type Verb = Annotated[str, Meta(pattern=r"\A[a-z][a-z0-9]*(\.[a-z][a-z0-9]*)+\Z")]
type Quantity = Annotated[int, Meta(ge=0)]
type Subject = Annotated[str, Meta(min_length=1, max_length=200)]
# stream tokens close the msgspec tag set, so the row column, the receipt roster, and the ledger discriminant
# all narrow to the same two literals and a third stream cannot land without its record.
type Stream = Literal["audit", "meter"]


class Actor(StrEnum):
    # `Actor` closes the actor-class vocabulary the party shape takes as its kind parameter, byte-identical to the
    # peer roster at `libs/typescript/data/.planning/journal/fact.md`, so one audit row reads the same class on both.
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
    # every measure this owner records, spelled ONCE and proved whole at install, so a missing `INSTRUMENTS` row refuses the
    # composition by name instead of raising `KeyError` out of the first append. UNITS ARE ABSENT BY LAW — the census owns each
    # row's unit, and a second spelling here exports a rescaled series nothing raises on.
    APPENDED = "rasm.journal.appended"
    DEDUPED = "rasm.journal.deduped"
    DEFERRED = "rasm.journal.deferred"
    ERASED = "rasm.journal.erased"
    GROOMED = "rasm.journal.groomed"
    DURATION = "rasm.journal.metered.duration"
    TALLY = "rasm.journal.metered.tally"
    VOLUME = "rasm.journal.metered.volume"


class Motion(StrEnum):
    # low-cardinality `kind` for series about the PLANE itself — a fact's series takes that record's own `kind` — so a board
    # groups plane health without learning which retention classes or resources exist.
    APPEND = "append"
    GROOM = "groom"
    ERASE = "erase"


# --- [CONSTANTS] ------------------------------------------------------------------------

# this owner's one name serving three vocabularies — receipt owner label, `DOMAINS` census segment, content-key format
# domain — spelled once, so a rename cannot leave a census pair or a key domain behind.
OWNER: Final[str] = "journal"

# NodaTime unix-tick geometry: `Hlc.physical_ticks` counts 100-ns intervals since the epoch. Every crossing into
# tick space runs integer arithmetic end to end — `time_ns` floor-divides, an aware instant and a window subtract
# through `timedelta` floor-division — so a float mantissa never rounds a stamp, a billing boundary, or a cutoff.
NANOS_PER_TICK: Final[int] = 100
TICKS_PER_MICRO: Final[int] = 1_000 // NANOS_PER_TICK
EPOCH: Final[datetime] = datetime(1970, 1, 1, tzinfo=UTC)
MICRO: Final[timedelta] = timedelta(microseconds=1)

# stream tokens the ledger discriminates rows by; each spells its own record's msgspec tag, so wire discriminant, row
# column, and receipt roster read one literal, and `STREAMS` DERIVES from the type so a third record cannot leave the
# receipt advertising a roster the family outgrew.
AUDIT_STREAM: Final[Stream] = "audit"
METER_STREAM: Final[Stream] = "meter"
STREAMS: Final[tuple[Stream, ...]] = get_args(Stream.__value__)

# every record defaults to this unstamped slot and the stamp cell seeds on it; `record` replaces it on admission, so its
# whole role is making the writer — never the caller — the mint authority.
UNSTAMPED: Final[Hlc] = Hlc(physical_ticks=0, logical=0)

# intake bound, batch width, and the patience a quiet surface still flushes on. The intake is BOUNDED and the writer
# AWAITS it: back-pressure suspends the producer, where a `send_nowait` drop loses billing truth nothing can replay.
INTAKE: Final[int] = 512
BATCH_WIDTH: Final[int] = 128
FLUSH_SECONDS: Final[float] = 2.0

# decorrelated-jitter append backoff, capped so a dead ledger costs a bounded cadence and a fleet of drains never
# re-hits a recovering ledger in lockstep; attempts never exhaust, because shedding a fact is the one failure this
# rail refuses.
BACKOFF_SECONDS: Final[float] = 0.1
BACKOFF_CEILING: Final[float] = 10.0

# AEAD geometry: 96-bit nonce is AES-GCM-SIV's own width, 256-bit data keys seal payloads, and the KEK is the
# 512-bit AES-256-SIV double key (one half MAC, one half CTR). `AEAD_SLOTS` bounds the worker threads the envelope
# crossing offloads onto, so an unbounded payload costs a slot rather than the loop.
NONCE_BYTES: Final[int] = 12
KEY_BITS: Final[int] = 256
KEK_BITS: Final[int] = 512
AEAD_SLOTS: Final[CapacityLimiter] = CapacityLimiter(8)

# exact-decimal money context: half-even at scale four, traps armed so an invalid or overflowing rate raises inside
# one rating fence rather than silently rounding a charge, and `FloatOperation` armed so a float operand REFUSES
# instead of contaminating a settlement. `Inexact` stays untrapped, since the terminal quantize IS the intended
# rounding and trapping it refuses every fractional price.
MONEY: Final[Context] = Context(
    prec=34, rounding=ROUND_HALF_EVEN, traps=[InvalidOperation, DivisionByZero, Overflow, FloatOperation]
)
SCALE: Final[Decimal] = Decimal("0.0001")

# hook points this owner registers at composition: the append point admits or refuses a fact BEFORE it occupies
# intake capacity, and the erase point fans the tombstone after key destruction lands, so a compliance observer
# subscribes to the fact instead of instrumenting this fold.
APPEND_POINT: Final[str] = "rasm.runtime.journal.append"
ERASE_POINT: Final[str] = "rasm.runtime.journal.erase"

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
    # ONE party shape whose kind VOCABULARY is the parameter: the actor half closes on `Actor` and refuses an
    # out-of-roster class at decode, the target half stays open because a target names whatever noun its verb
    # touched. A closed class column seated BESIDE an open one on the same record puts two answers on the actor's
    # class, and the column no producer fills then defaults its way into every stored row as a lie.
    kind: K
    key: str
    parent: str | None = None


class AuditFact(Struct, tag=AUDIT_STREAM, frozen=True, omit_defaults=True):
    # class-level slots ride the TYPE and never the wire: `stream` is the ledger discriminant, `gate` the
    # admission point this record crosses, and the billing pair answers absent so `_rowed` stays total over the
    # family. Declaring them here is what keeps every consuming fold total — a third stream lands as one more
    # record with its own slots and edits no fold.
    stream: ClassVar[Stream] = AUDIT_STREAM
    gate: ClassVar[str | None] = APPEND_POINT
    resource: ClassVar[Resource | None] = None
    quantity: ClassVar[Quantity] = 0

    action: Verb
    actor: Party[Actor]
    target: Party[str]
    retention: Retain
    change: tuple[Change, ...] = ()
    # subject keys this fact touched — the ONE index the portability export walks and the erasure keys on, making a
    # data-subject read an index scan instead of a full-stream crawl.
    subjects: tuple[Subject, ...] = ()
    tenant: Tenant | None = None
    # `record` REPLACES this slot at admission; a caller-threaded coordinate would let two producers mint one content
    # key and dedup genuine evidence away as a redelivery.
    stamp: Hlc = UNSTAMPED

    @property
    def kind(self) -> str:
        return self.retention.value

    @property
    def measures(self) -> Mapping[str, float]:
        return {Series.APPENDED: 1.0}


class MeterFact(Struct, tag=METER_STREAM, frozen=True, omit_defaults=True):
    # metering carries NO admission point and no retention column: a charge is a quantity the rating fold prices
    # rather than a policy surface a subscriber starves, and billing truth is regulatory by constitution. Both
    # answers ride the type, so the drain reads one shape across the family and neither is a caller's to weaken.
    stream: ClassVar[Stream] = METER_STREAM
    gate: ClassVar[str | None] = None
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
        # one landed row plus the quantity onto the series its `RESOURCES` row names; resources sharing a unit share a
        # series and separate on `kind`, since folding milliseconds into a byte counter exports an uninterpretable sum.
        # `Mapping` is invariant in its key, so the declared type stays `str` and a `Series` member satisfies both.
        return {Series.APPENDED: 1.0, RESOURCES[self.resource]: float(self.quantity)}


type Fact = AuditFact | MeterFact
type Recordable = Fact | Iterable[Fact]

# one converter per closed family, seated with the owner it converts, on the msgpack arm the branch content-key law
# fixes — one deterministic `msgspec.msgpack` encode feeds one XxHash128, so this preimage matches the canonical
# arm `evidence/identity#IDENTITY` already mints and JSON never enters identity. `order="deterministic"` fixes key
# order, so encoded payload IS the preimage and a replayed fact keys byte-identically; no `enc_hook` rides the
# encoder, and every field is a native msgspec type, so encoding is TOTAL over admitted facts by construction.
# `DECODE` is what a ledger composes to lift a stored payload back into the family, and its `Meta` bounds re-run
# over every historical row at scan — a bound may widen once rows landed, never tighten.
ENCODE: Final[Callable[[Fact], bytes]] = Encoder(order="deterministic").encode
DECODE: Final[Callable[[bytes], Fact]] = Decoder(type=Fact).decode


class FactRow(Struct, frozen=True):
    # ledger projection: `key` content-keys its own encoded payload, so an at-least-once landing dedups structurally;
    # each stamp rides INSIDE its payload, so two genuinely distinct facts never collide on one key. Every column a
    # ledger PREDICATES on is lifted out of the opaque payload — `subjects` for the portability and erasure reads,
    # `retention` for the groom, `stamp` and `stream` for the billing window, and the metering pair for the rollup —
    # so `tallied` pushes its group-by into whatever engine indexes the window. Leaving `resource` and `quantity`
    # inside the payload instead makes that member a per-row decode wearing a pushdown's name, and a settlement month
    # then allocates one object per metered fact to produce a handful of slots. The pair answers absent on the audit
    # stream, and no meter tally reaches those rows: `Period.stream` narrows the scan to one stream first.
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


# machine-readable seam target: the ledger implementer derives its table DDL, the tag discriminant, and per-field
# presence from this `$defs`-bearing schema pair — payload family beside durable row — never from prose, and the
# cross-branch `Resource` parity clause gains a diffable form, so a forked roster fails a schema comparison
# instead of surviving as prose drift.
SCHEMA: Final[tuple[tuple[dict[str, object], ...], dict[str, object]]] = schema_components((Fact, FactRow))


class Landing(Struct, frozen=True, gc=False):
    # ledger verdict per batch, and the ONLY surface reporting a short write: the two halves PARTITION the offered
    # keys — disjoint, their union the whole set — and any other answer is a failed attempt rather than a landing.
    # A ledger silently dropping half a batch otherwise reads as success and sheds evidence, and one answering a
    # key on both halves, or a key from another batch, satisfies a bare count while naming rows it never held.
    # Both halves carry the KEYS rather than tallies, because the drain projects the accepted half alone: the retry
    # above them never exhausts, so a batch re-offered after a lost acknowledgement must cost one absorbed
    # duplicate rather than a second charge and a second audit line, and a count names no row to filter on. An
    # upsert reporting one fused output tally would satisfy the completeness sum while claiming zero redelivery,
    # and zero redelivery is what a wedged retry re-offering one window forever looks like from the drain's series.
    # Implementers whose engine reports counts alone read the matched keys before the commit rather than inferring
    # them, since keys are unique per batch by the stamp riding inside each payload.
    accepted: Block[ContentKey]
    duplicate: Block[ContentKey]


class JournalDrain(Struct, frozen=True, gc=False):
    # terminal drain evidence, associative so the loop folds window after window without a mutable tally. A drain
    # returning nothing would leave the one plane whose thesis IS evidence unable to report its own throughput.
    # `landed` counts the ACCEPTED half alone and `deduped` the matched half, so the two never overlap and a
    # session's throughput reads honestly across a retry that re-offered a window it had already committed.
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
    # tenant-scoped custody identity — equal subject strings under two tenants share no key material, export row, or
    # erasure — and `aad` binds every envelope to this exact identity, so a ciphertext lifted onto another subject
    # fails its tag rather than opening under a live key that belongs to someone else.
    tenant: Tenant
    subject: Subject

    @property
    def aad(self) -> bytes:
        return f"rasm.subject:{self.tenant}:{self.subject}".encode()


class Custody(Struct, frozen=True):
    # KEK custody as a policy VALUE rather than a second port, async whole exactly as the ledger is — a remote KMS
    # arm is a network call no synchronous wrap may run on the loop — so the two shipped instances are two named
    # values of one shape and this ledger stores whatever wrapped form an arm produced. Both wrap under deterministic
    # AES-SIV bound to the subject aad over a `KEK_BITS` key: key-wrap needs no nonce custody and determinism leaks
    # nothing over fresh random keys, where GCM nonce reuse on the KEK path is catastrophic and RFC 3394 keywrap
    # carries no associated data at all. `local` holds material a root already resolved; `vault` reads the deployment's
    # own ladder per call, and a third posture is one more instance rather than a field on either.
    wrap: Callable[[bytes, bytes], Awaitable[RuntimeRail[bytes]]]
    unwrap: Callable[[bytes, bytes], Awaitable[RuntimeRail[bytes]]]

    @classmethod
    def local(cls, kek: bytes) -> Self:
        async def wrapped(material: bytes, aad: bytes) -> RuntimeRail[bytes]:
            return _crypto("journal.wrap", lambda: AESSIV(kek).encrypt(material, [aad]))

        async def unwrapped(held: bytes, aad: bytes) -> RuntimeRail[bytes]:
            return _crypto("journal.unwrap", lambda: AESSIV(kek).decrypt(held, [aad]))

        return cls(wrap=wrapped, unwrap=unwrapped)

    @classmethod
    def vault(cls, boundary: SecretBoundary, service: str) -> Self:
        # the deployment-custody posture beside `local`: the KEK reaches this arm through the ONE credential reader
        # `execution/admission#SETTINGS` owns, so a keystore, a cloud vault, and a mounted secrets directory all serve
        # it and no arm here learns which tier answered. Resolution runs PER CALL rather than captured at bind, so a
        # rotated KEK reaches the next wrap with no rebind and no composition holds material past its rotation — the
        # capability a `local(kek)` bind structurally cannot carry. The resolver already folds each tier through the
        # `RetryClass.SECRET` row, so no second retry envelope wraps it here, where one would multiply the ladder's
        # own attempts against a locked keystore.
        async def material() -> RuntimeRail[bytes]:
            # absence is a REFUSAL, never a fallback: `Ok(Nothing)` means the deployment named no KEK for this
            # service, and minting a substitute would render every envelope written under the real key permanently
            # unopenable while every wrap read clean.
            resolved = await boundary.resolve(service, shape=SecretShape.TOKEN)
            return resolved.bind(
                lambda held: held.map(lambda secret: _kek(secret.get_secret_value())).default_with(
                    lambda: Error(BoundaryFault(config=("journal.custody", f"no key material bound for {service!r}")))
                )
            )

        async def wrapped(payload: bytes, aad: bytes) -> RuntimeRail[bytes]:
            return (await material()).bind(lambda kek: _crypto("journal.wrap", lambda: AESSIV(kek).encrypt(payload, [aad])))

        async def unwrapped(held: bytes, aad: bytes) -> RuntimeRail[bytes]:
            return (await material()).bind(lambda kek: _crypto("journal.unwrap", lambda: AESSIV(kek).decrypt(held, [aad])))

        return cls(wrap=wrapped, unwrap=unwrapped)


class Period(Struct, frozen=True, gc=False):
    # billing coordinate: one stream over a half-open stamp window, optionally narrowed to one tenant. Tenancy is a
    # COLUMN here rather than a post-filter, so settling one customer reads that customer's rows alone instead of
    # scanning every tenant's window to discard the rest.
    stream: Stream
    since: Hlc
    until: Hlc
    tenant: Tenant | None = None

    @classmethod
    def of(cls, stream: Stream, since: datetime, until: datetime, tenant: Tenant | None = None) -> RuntimeRail[Self]:
        # billing callers reach this ONE construction: a settlement window names wall-clock instants and the stream
        # sorts on ticks, so conversion lands here rather than leaving every consumer to re-derive it and disagree
        # with the groom horizon about what an instant means. Ordering admits HERE too — a half-open window whose
        # lower bound sorts above its upper selects no row at all, so an inverted settlement reads as a zero
        # invoice the customer and the ledger both agree on, the one billing defect no later fold can see.
        return at(since).map2(at(until), lambda lower, upper: (lower, upper)).bind(
            lambda bounds: Ok(cls(stream=stream, since=bounds[0], until=bounds[1], tenant=tenant))
            if bounds[0].packed <= bounds[1].packed
            else Error(BoundaryFault(config=("journal.period", "window lower bound sorts above its upper")))
        )


@tagged_union(frozen=True)
class Scan:
    # one read coordinate over two shapes: a billing period reads one stream across a half-open stamp window, a
    # portability export reads every fact one subject touched. Both ride the ledger's `scanned`/`tallied` pair,
    # so no consumer hand-mints a query and the export proves the same index the billing read walks.
    tag: Literal["period", "subject"] = tag()
    period: Period = case()
    subject: SubjectKey = case()


class Bound(Struct, frozen=True):
    # what one composition binds, as a NAMED carrier. `service` is the emitter identity the durable rows partition on,
    # injected exactly as the telemetry install takes its resource — a worker floor, an offline job, and a daemon each
    # write under the identity they run as, where a module constant read at drain time claims one for all three.
    ledger: "Ledger"
    custody: Custody
    service: str


class JournalReceipt(Struct, frozen=True):
    ledger: str
    service: str
    streams: tuple[Stream, ...]
    classes: tuple[Retain, ...]


# --- [TABLES] ---------------------------------------------------------------------------

# one row per metered resource naming the series its quantity projects onto; the unit belongs to the census row that
# mounts the instrument, so a row can never mount a spelling its census counterpart disagrees with.
RESOURCES: Final[Map[Resource, Series]] = Map.of_seq([
    (Resource.COMPUTE, Series.DURATION),
    (Resource.RECORD, Series.TALLY),
    (Resource.REQUEST, Series.TALLY),
    (Resource.STORAGE, Series.VOLUME),
    (Resource.TOKEN, Series.TALLY),
])

# one window per class and the single edit site every aging surface reads. `PERMANENT` carries `Nothing`, so the
# horizon emits no cutoff for it and a permanent row is unreachable by any groom rather than gated by a check.
WINDOWS: Final[Map[Retain, Option[timedelta]]] = Map.of_seq([
    (Retain.EPHEMERAL, Some(timedelta(days=7))),
    (Retain.OPERATIONAL, Some(timedelta(days=90))),
    (Retain.REGULATORY, Some(timedelta(days=2555))),
    (Retain.PERMANENT, Nothing),
])

# points this owner registers at composition, as rows rather than a hand-repeated pair of calls: the append gate is
# typed by the record that names it through `gate`, and the erase point observes a tombstone. A new point is one row.
POINTS: Final[Block[HookPoint[Struct]]] = Block.of_seq([
    HookPoint(id=APPEND_POINT, payload=AuditFact, modality=Modality.VETO),
    HookPoint(id=ERASE_POINT, payload=Tombstone, modality=Modality.OBSERVE),
])

# derived-line field policy, classified by KEY NAME at every depth: diff values mask, party and subject identifiers
# hash to stable correlation tokens, and the pointer path stays legible since a path names a field rather than carrying
# its value — the keep-all policy would publish into the log plane exactly the material the shredder makes unreadable.
FACT_REDACTION: Final[Redaction] = Redaction(
    classified=Map.of_seq([("prior", "mask"), ("next", "mask"), ("key", "hash"), ("subjects", "hash")])
)

# --- [OPERATIONS] -----------------------------------------------------------------------


def _sampled() -> Hlc:
    return Hlc(physical_ticks=time_ns() // NANOS_PER_TICK, logical=0)


def _ticks(span: timedelta) -> int:
    return span // MICRO * TICKS_PER_MICRO


def at(moment: datetime) -> RuntimeRail[Hlc]:
    # wall clock crosses into stamp space exactly HERE, refusing a naive instant rather than normalising one: a
    # naive value reads as LOCAL time, so a month boundary silently shifts by the host offset and settles the
    # wrong rows. The OFFSET is what awareness means, never the presence of a `tzinfo` object — a custom
    # implementation whose `utcoffset` answers `None` is naive by the language's own rule, and the `EPOCH`
    # subtraction raises `TypeError` on it where the attribute test alone admits it. The subtraction stays
    # integer µs-then-ticks, so a settlement boundary lands exact where a float `timestamp()` mantissa rounds it.
    return (
        Ok(Hlc(physical_ticks=(moment - EPOCH) // MICRO * TICKS_PER_MICRO, logical=0))
        if moment.utcoffset() is not None
        else Error(BoundaryFault(config=("journal.instant", "naive datetime carries no offset")))
    )


def horizon(now: Hlc) -> Map[Retain, Hlc]:
    # groom cutoffs in the stream's OWN ordering coordinate: a finite window yields the stamp below which its rows
    # reclaim, `PERMANENT` emits no key at all, and the floor clamps so an unreachable window never mints a negative
    # physical half. Taking `now` as a stamp keeps cutoff and stamp on one time base — a caller instant admits a
    # clock the stream never sorted on — and the ledger then compares stamps and parses no date.
    return Map.of_seq(
        (clazz, Hlc(physical_ticks=max(now.physical_ticks - _ticks(window), 0), logical=0))
        for clazz, held in WINDOWS.items()
        for window in held.to_list()
    )


def _kek(text: str) -> RuntimeRail[bytes]:
    # a secret store holds text, so the KEK travels as its hex render — the one text encoding this estate freezes key
    # material in — and decodes here rather than at the AEAD call. Width proves in the same expression: `AESSIV`
    # accepts 256 and 512 bits, so a KEK rotated to the narrower width would wrap cleanly under a primitive this
    # plane never declared, and the refusal names the coordinate instead of surfacing as a length raise mid-batch.
    # The fence catches `ValueError` alone and touches no `cryptography` name, so a composition that seals nothing
    # never reifies the native tier to read a key it will not use.
    return boundary("journal.kek", lambda: bytes.fromhex(text), catch=ValueError).bind(
        lambda kek: Ok(kek)
        if len(kek) * 8 == KEK_BITS
        else Error(BoundaryFault(config=("journal.custody", f"kek width {len(kek) * 8} != {KEK_BITS}")))
    )


def _crypto[T](subject: str, run: Callable[[], T]) -> RuntimeRail[T]:
    # ONE native crossing every AEAD call composes: the outer fence reifies the lazy `cryptography` proxy under the
    # default surface, so an absent or unbuildable build classifies `import_` on the rail at whichever entry touches
    # it first — seal, wrap, unwrap, or open alike — and the narrow tuple applies only once the primitive is in
    # hand. Naming the tuple as a module `Final` instead dereferences the proxy at import and reifies the native
    # tier in every composition that seals nothing; naming it per call site leaves the open-only path unfenced,
    # since the catch argument evaluates before its own fence exists.
    return boundary(subject, lambda: (InvalidTag, UnsupportedAlgorithm, ValueError, OverflowError)).bind(
        lambda catch: boundary(subject, run, catch=catch)
    )


async def _offloaded[T](subject: str, run: Callable[[], T]) -> RuntimeRail[T]:
    # envelope AEAD leaves the loop: an unbounded payload seals on a worker thread bounded by `AEAD_SLOTS`, so a
    # multi-megabyte crossing costs a slot rather than stalling the drain sharing this scheduler. The inner sync
    # fence classifies every crypto raise, the outer fence the offload itself, and one `bind` flattens the pair.
    lifted = await async_boundary(subject, lambda: anyio.to_thread.run_sync(lambda: _crypto(subject, run), limiter=AEAD_SLOTS))
    return lifted.bind(lambda railed: railed)


def _censused() -> RuntimeRail[Block[Series]]:
    # composition-root census gate: each `Series` member resolves its `(domain, measure)` pair against the metrics
    # owner's published `MEASURES`, so a measure without its `INSTRUMENTS` row refuses the install by name instead of
    # raising `KeyError` out of a drain nobody is watching; resolving that map IS the whole-descriptor proof, which is
    # why no unit is spelled on this page.
    rows = Block.of_seq(Series)
    unmet = rows.filter(lambda row: MEASURES.try_find((OWNER, row.value)).is_none())
    return Ok(rows) if unmet.is_empty() else Error(BoundaryFault(config=("journal.census", f"measures unrostered: {', '.join(unmet)}")))
```

## [03]-[LEDGER]

- Owner: `Ledger` is the one durable port and `Journal` the one writer; the drain TAKES the receive handle out of custody, so single consumership is structural — a second concurrent drain on one scope refuses instead of splitting the stream — and the stamp cell, the batch fold, and the retry cadence have exactly one reader. `_rowed` is the one projection into the ledger — content key, stamp, stream token, service, class, subject index, metering pair, and opaque msgpack payload — so the durable schema is a value this page owns, `SCHEMA` hands the implementer that contract as machine-readable data, no consumer re-derives a column, and the projection is TOTAL, since a rail there opens a shed path under the one law this plane refuses.
- Owner: every coordinate a ledger PREDICATES, GROUPS, or PARTITIONS on lifts into its own row column and only the fact body stays opaque — subject index, retention class, stamp window, and the `(resource, quantity)` metering pair alike — so `tallied` is a pushed-down group-by rather than a decode hop wearing that name, and `Aggregate.rolled` publishes the same algebra for an engine whose reader carries no grouped form. Withholding a column a port member needs turns that member into the full-window materialization the delegation exists to avoid.
- Owner: `Landing.accepted` NAMES the rows the plane did not already hold and `duplicate` names the redeliveries the content key matched, both as keys, because the drain's line, its series, and its metered quantities fire off the accepted half ALONE — the append retry never exhausts, so a batch replayed after a lost acknowledgement costs one absorbed duplicate rather than a doubled charge and a doubled audit line, and a half reported as a bare tally names no row that filter can read. One fused count across both arms satisfies the completeness sum while claiming zero redelivery, and zero redelivery is indistinguishable from a wedged retry re-offering one window forever; `rasm.journal.deduped` carries that half off the drain, so an inflated `accepted` deletes the one signal proving at-least-once delivery is happening.
- Cases: every port member awaits, because a ledger writes durably and the drain runs on the loop — a synchronous member stalls the scheduler for the whole batch, and the branch admits no on-loop blocking arm; the install proof refuses a member present yet not a coroutine function, which otherwise faults inside the retry indistinguishable from a dead ledger. `_chained` is the one async bind carrying a settled rail into an awaited continuation, `_resolved` composing it over the bound carrier, so an unbound scope refuses once and a custody-and-ledger chain short-circuits with no hand-repeated match.
- Law: `install` proves the census and the port structurally, registers the point rows, and binds only then — a measure or a member missing refuses at the bind naming it, where an unchecked bind defers the failure into the unbounded retry and reads as a dead ledger forever. Re-entry returns the standing receipt, so a second composition-root pass never swaps a ledger out from under a live drain; point registration latches per scope, because ids are composition-unique and the registry ships no retirement.
- Law: `closed` retires custody with the intake and MARKS the scope retired, so a composition that shuts down re-arms by installing again rather than adopting a closed stream every `record` then rails `resource` against, and the mark is what lets `record` separate a plane that died from one that never existed — `_pointed` cannot, latching past every retirement by design. Unlanded rows survive that retirement, since a shutdown owing them is exactly when a caller reads them.
- Law: `record` resolves THREE intake states and never two — an armed scope sends, a scope whose custody `closed` RETIRED refuses with the `config` fault naming the port, and a scope no composition ever installed folds to `Ok(0)`. That third state is a deployment DECLARATION that this process journals nothing: its facts drop lawfully, so every producer binds the same rail into its verdict whatever the deployment armed, a refusal on an armed plane still surfaces, and an unarmed one costs one map read. The unarmed fold records no series either — a drop counter prices the hot path of a composition that asked for no plane, and the install census proving this owner's measures never ran on that scope. Collapsing the pair into one refusal fails every producer's verdict wherever evidence was never deployed; collapsing it the other way renders retired custody — a producer outliving its own plane, which is a real fault — as lawful silence.
- Law: a producer records at the nearest ASYNC fold that OWNS the fact, and a synchronous `contribute` never records — recording suspends by law, so a sync projection can only shed exactly what the never-shed rail refuses to shed, and the derived series is already `contribute`'s own half. An owner whose whole surface is synchronous mints its awaitable leg over the band hop its callers hand-roll rather than moving the record onto the loop-blocking side. A producer's `action` spells `<domain>.<operation>` — the same `domain` segment its metric projection carries beside the operation its own dispatch names — so one verb greps against the series its evidence twin emitted and no central verb registry stands between them.
- Entry: `record` folds arity off the value so a lone fact and a batch of either stream take one entry, and stamps every fact it admits; an EMPTY offer is admitted arity rather than a refusal — a metering fan that priced nothing holds no row to charge, which is what a zero-quantity producer leg settles to — so it fires no gate, ticks no stamp, and answers the count it landed, while a RETIRED scope refuses it exactly as it refuses a full batch, since that fault names a producer outliving its plane and no batch width makes that lawful; `drained` is the composition-root coroutine a task group starts — `tg.start` blocks on its readiness signal, so no producer suspends into an intake nothing reads, and the root reads the terminal tally off the child handle — and `closed` is its lossless counterpart. Recording is async by law — the send suspends under back-pressure, and no synchronous spelling can suspend — so a sync producer re-enters the loop through the portal bridge exactly as every other foreign-thread crossing does. `drained` and `closed` refuse an unbound scope with a `config` fault naming the port; `record` splits that absence under the three-state law above.
- Auto: the ledger lands the batch FIRST and its `Landing` must PARTITION the offered keys — disjoint halves whose union is the whole set — so a short write, an overlapping half, and a foreign key all retry rather than projecting; only an exact landing projects, only its ACCEPTED half does, and an observer never reads a fact the durable plane refused or already held. Retry attempts never exhaust and the decorrelated-jitter delay caps, so a dead ledger costs a bounded cadence a fleet never synchronizes on, while the bounded intake propagates pressure back through the suspended writer; the deferral counter reports the ledger and the drain's occupancy probe reports intake depth with suspended senders, so a full intake behind a healthy ledger is visible pressure too.
- Auto: shutdown closes the intake and awaits the drain, never cancels it — `anyio` delivers every buffered fact after the last send end closes, so the partial window flushes, `drained` returns its tally, and nothing in flight sheds. Roots that must nevertheless bound the wait wrap the await in their own `CancelScope` and read `pending` after it trips: a tripped scope returns no value, so the drain's terminal deposits BOTH the batch it was retrying and the checkpoint-free sweep of whatever still sat in the buffer, and every fact either landed or is named. Deadline parameters here instead re-thread the cancellation a scope already owns and cap the steady-state retry the never-shed law forbids capping.
- Receipt: `install` deposits its receipt on the `observability/hooks#HOOKS` install record, so a support bundle answers which durable plane a composition wrote to, under which vocabulary, and as which service, without this owner minting a second custody surface for the bundle to read.
- Law: `_pending` ACCUMULATES and settles by key — `_owed` appends and `_settled` removes exactly the keys a landing covered, so a scope re-installed after `closed` still owes what its prior drain never landed. Replacing the slot hands the next session's first batch that debt to overwrite, and a blanket clear erases it on the first success, both shedding evidence on the one plane whose whole thesis is that nothing sheds.
- Growth: a new durable coordinate is one `FactRow` column reaching the ledger and the row projection; a new read shape is one `Scan` case with its ledger arm; a new drain posture is one flow or backoff constant; a new ledger family is one implementer of the port with zero edits here.
- Boundary: a `Ledger` implementer's own landing path records NOTHING — `landed` reaches durability through whatever commit surface that implementer composes, so a producer leg seated on that surface re-enters `record` for every batch it lands and the stream feeds itself without bound. The implementer's composed owners therefore discriminate the ledger's OWN relations from a caller's and record only the caller's, and the durable plane's emptiness cannot do it for them: a journal commit is indistinguishable from a caller's commit by residence alone, so the discriminant is the relation identity the ledger's tables declare at open.
- Boundary: this owner opens no connection, mints no statement, and names no engine — the ledger executes every landing, scan, tally, and reclaim through its own mechanism, so retention, compaction, and rollup ride machinery a ledger already carries and no worker or scheduler surface enters this branch for telemetry. Append gating admits or refuses and never transforms: the veto fold's returned payload is discarded by law, because a subscriber rewriting evidence makes the plane it observes a second author.

```python signature
# --- [OPERATIONS] -----------------------------------------------------------------------


def _rowed(fact: Fact, service: str) -> FactRow:
    # TOTAL by construction: the payload is already bytes, so the key derives through the bare `ContentIdentity.key`
    # accessor rather than the railed entry, and no fact can be lost to a rail the drain would have to choose between
    # dropping and stalling on. Every column resolves off the record's own projection, so the fold carries no stream
    # branch and a third record inherits it whole.
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


def _fenced(subject: str, run: Callable[[], object], scope: ScopeKey) -> None:
    # every derived write in this owner crosses HERE, fenced so it can never fault the plane that owns the truth: a
    # render, record, or sink fault lands as its own rejected receipt, because a dashboard gap never justifies
    # stalling an append.
    boundary(subject, run).swap().map(lambda fault: Signals.emit(Receipt.of(OWNER, fault), OPEN, scope=scope))


def _series(measures: Mapping[str, float], kind: str, scope: ScopeKey) -> None:
    # `Series` members ARE their measure names, so the census-proved vocabulary reaches the recorder unchanged and
    # no call site on this page carries a bare metric literal.
    _fenced("journal.series", lambda: Metrics.record(measures, domain=OWNER, kind=kind, scope=scope), scope)


def _projected(fact: Fact, scope: ScopeKey) -> None:
    # structured lines render the audit vocabulary through `to_builtins`, so the nested diff cases, parties, and
    # stamp arrive as mappings the redaction policy descends — a shallow struct projection leaves every classified
    # field one level down unreachable and publishes it verbatim.
    _series(fact.measures, fact.kind, scope)
    _fenced(
        "journal.line",
        lambda: Signals.emit(Receipt.of(OWNER, ("emitted", fact.kind, to_builtins(fact, str_keys=True))), FACT_REDACTION, scope=scope),
        scope,
    )


async def _batched(receive: MemoryObjectReceiveStream[Fact]) -> AsyncIterator[Block[Fact]]:
    # width-and-patience window `anyio` ships no combinator for — the OTel batch processors carrying these knobs
    # shed at queue-full on their own thread, refused under the never-shed law: block for the first fact, then
    # gather up to the remaining width inside one patience scope, so a busy surface fills whole batches and a quiet
    # one still flushes on latency. Both exits are lossless — a `receive` that already took its item returns it
    # before the scope's cancellation reaches the next checkpoint, and `EndOfStream` closes the window rather than
    # escaping it and abandoning the facts already taken from the buffer.
    async for head in receive:  # Exemption: the batching window is the statement-bearing kernel anyio does not carry
        held = [head]
        with anyio.move_on_after(FLUSH_SECONDS):
            while len(held) < BATCH_WIDTH:
                try:
                    held.append(await receive.receive())
                except anyio.EndOfStream:
                    break
        yield Block.of_seq(held)


def _swept(receive: MemoryObjectReceiveStream[Fact], service: str) -> Block[FactRow]:
    # checkpoint-free buffer sweep the drain's terminal runs even inside a tripped scope: `receive_nowait` takes an
    # item without awaiting AND unwinds one suspended sender into the buffer per call, so a bounded shutdown empties
    # both the queue and the producers back-pressured behind it into the unlanded roster — absent it every fact still
    # queued behind the batch in flight sheds silently, exactly the loss the roster exists to make visible.
    held: list[FactRow] = []
    while True:  # Exemption: the sweep must run under cancellation, where no awaiting form completes
        try:
            held.append(_rowed(receive.receive_nowait(), service))
        except (anyio.WouldBlock, anyio.EndOfStream):
            return Block.of_seq(held)


async def _chained[T, U](rail: RuntimeRail[T], step: Callable[[T], Awaitable[RuntimeRail[U]]]) -> RuntimeRail[U]:
    # `_chained` carries a settled rail into an awaited continuation — the async `bind` the substrate does not
    # ship — so a custody-and-ledger chain short-circuits an `Error` with no hand-repeated match per step.
    match rail:
        case Result(tag="ok", ok=value):
            return await step(value)
        case refused:
            return Error(refused.error)


async def _resolved[T](scope: ScopeKey, run: Callable[["Bound"], Awaitable[RuntimeRail[T]]]) -> RuntimeRail[T]:
    # every ledger-touching entry composes this bind over the bound carrier, so an unbound scope refuses once
    # instead of at six hand-repeated sites that could drift apart.
    return await _chained(Journal.bound(scope), run)


def _proven(ledger: object) -> RuntimeRail["Ledger"]:
    # structural admission over the port's OWN declared roster, presence AND awaitability both: a member satisfied
    # by a plain `def` passes a presence probe, then faults inside the unbounded retry where it is indistinguishable
    # from a dead ledger — refusal here names the member instead, and a member added to `Ledger` refuses every
    # stale implementer with no edit here.
    unmet = Block.of_seq(sorted(member for member in get_protocol_members(Ledger) if not iscoroutinefunction(getattr(ledger, member, None))))
    return Ok(ledger) if unmet.is_empty() else Error(BoundaryFault(config=("journal.ledger", f"port members unmet: {', '.join(unmet)}")))


def _partitions(landing: Landing, rows: Block[FactRow]) -> bool:
    # a landing accounts for every offered row EXACTLY: the two halves are disjoint, their union IS the offered key
    # set, and neither repeats a key inside itself. The completeness SUM alone proves none of that — a ledger
    # answering one key on both halves, or a key from a neighbouring batch, satisfies the arithmetic while the
    # accepted half names rows the plane never held, and the drain then charges and audits off that half. Keys are
    # unique per batch by the stamp riding inside each payload, so the cardinality test IS the intra-half guard.
    offered = frozenset(row.key for row in rows)
    admitted, matched = frozenset(landing.accepted), frozenset(landing.duplicate)
    return (
        admitted.isdisjoint(matched)
        and admitted | matched == offered
        and len(landing.accepted) + len(landing.duplicate) == len(rows)
    )


def _tenanted(fact: Fact) -> Fact:
    # the ONE baggage reader on this plane: a producer leaving `tenant` unset is asking the writer to resolve it, so
    # the ambient W3C entry the metrics attribute fold already keys on fills the slot here instead of at every
    # producer. A fact arriving WITH tenancy passes through untouched — recording on behalf of one tenant from inside
    # another's context is the case a re-read silently overwrites — and the guard reads the slot before the context,
    # so an attributed batch never pays a baggage lookup per fact. The metric tenant budget stays out: it bounds a
    # series value axis and this plane bounds no cardinality.
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
    # one durable seam a composition binds and S0 never satisfies: the columnar owner supplies rows, this rail
    # supplies facts, every member awaits, and every member returns the branch rail so a ledger fault reaches the
    # drain as data. `SCHEMA` carries the row contract's machine form; the behavioral half reads — `landed` is
    # idempotent on `FactRow.key` and its `Landing` PARTITIONS the offered keys, accepted NAMING the keys the
    # plane did not hold and duplicate the keys it matched, every other answer retried by the drain; an engine reporting
    # counts alone resolves the matched keys BEFORE its commit, since after one every offered key reads present;
    # `scanned` lifts
    # stored payloads through `DECODE`, so this family owner alone admits its own wire; `tallied` is the rollup arm —
    # a ledger's own group-by over the `(tenant, resource)` columns `FactRow` lifts beside the window `scanned`
    # walks, returned as the `Priced`-keyed monoid rows `rolled` folds, so a settled period never materializes a
    # window in this process to sum two integers, and an engine carrying no grouped read folds `Aggregate.rolled`
    # over its own scan rather than minting a second rollup; `groomed` reclaims every row whose stamp sorts
    # below its class cutoff and reports the count, a class absent from the map untouched; `claimed` is atomic
    # insert-or-return-standing on the custody row; `held` reads the standing wrapped material; `destroyed`
    # empties the custody slot, persists the journal-minted stone beside it, and answers `Nothing` when no live
    # key existed — a ledger mints neither stamp nor identity.
    async def landed(self, rows: Block[FactRow], /) -> RuntimeRail[Landing]: ...
    async def scanned(self, scan: Scan, /) -> RuntimeRail[Block[Fact]]: ...
    async def tallied(self, scan: Scan, /) -> RuntimeRail["Billed"]: ...
    async def groomed(self, horizon: Map[Retain, Hlc], /) -> RuntimeRail[Groomed]: ...
    async def claimed(self, subject: SubjectKey, wrapped: bytes, /) -> RuntimeRail[bytes]: ...
    async def held(self, subject: SubjectKey, /) -> RuntimeRail[Option[bytes]]: ...
    async def destroyed(self, stone: Tombstone, /) -> RuntimeRail[Option[Tombstone]]: ...


class Journal:
    # composition-owned custody, deliberately without the process latch every sibling install carries — a ledger is
    # a bound value each composition supplies, never an SDK singleton to adopt. The stamp cell is the one exception
    # and is process-wide by law: an HLC names this NODE's clock, so two compositions sharing a process must share the
    # successor or their rows interleave under coordinates neither can order. Serializing that cell and each map
    # read-modify-write is the gate's whole custody; bound carrier and intake handle read lock-free off immutable
    # snapshots, so a recording producer never queues behind a process acquire.
    _bound: ClassVar[Map[ScopeKey, Bound]] = Map.empty()
    _intake: ClassVar[Map[ScopeKey, MemoryObjectSendStream[Fact]]] = Map.empty()
    _drain: ClassVar[Map[ScopeKey, MemoryObjectReceiveStream[Fact]]] = Map.empty()
    _pending: ClassVar[Map[ScopeKey, Block[FactRow]]] = Map.empty()
    _receipts: ClassVar[Map[ScopeKey, JournalReceipt]] = Map.empty()
    _pointed: ClassVar[frozenset[ScopeKey]] = frozenset()
    # the one coordinate separating a scope whose custody was RETIRED from a scope no root ever installed: both
    # answer an absent intake, and only the first is a fault. `closed` marks, `_bind` clears — so a re-armed scope
    # sends again — while `_pointed` cannot serve here, since a point latch survives every retirement by design.
    _retired: ClassVar[frozenset[ScopeKey]] = frozenset()
    _stamp: ClassVar[Hlc] = UNSTAMPED
    # per-scope install lock, distinct from the custody gate BY LAW: install registers points through `Hooks`,
    # which takes its own lock, so holding `_gate` across that call would mint the two-lock ordering `_bind`'s
    # outside-the-gate deposit exists to foreclose. Serializing the whole install per scope here instead makes
    # census, proof, registration, and bind one transaction while `_gate` still covers map mutation alone.
    _installing: ClassVar[Map[ScopeKey, RLock]] = Map.empty()
    _gate = RLock()

    @classmethod
    def stamped(cls, causal: Option[CausalFrame] = Nothing) -> Hlc:
        # one stamp mint: merge the inbound cause, join this process's physical sample, tick the successor. This gate
        # makes that coordinate a total order — two free-threaded recorders take distinct successors, where an
        # unsynchronized read-modify-write mints one stamp twice and collapses two rows onto one key.
        observed = causal.map(lambda frame: frame.hlc).default_value(cls._stamp)
        with cls._gate:
            cls._stamp = Hlc.merge(cls._stamp, observed).tick(_sampled())
            return cls._stamp

    @classmethod
    def install(
        cls, ledger: Ledger, custody: Custody, *, service: str = SCOPES[Scope.JOURNAL], scope: ScopeKey = DEFAULT_SCOPE
    ) -> RuntimeRail[JournalReceipt]:
        # order is the whole correctness of this entry: a standing receipt short-circuits before anything runs, the
        # census and the port prove, the points register, and ONLY a clean registration binds. Binding first would
        # leave a refused install holding a live ledger beside an orphaned intake no drain reads, and probing
        # inside the match subject would run every proof on the re-entrant path. The scope's own install lock makes
        # that order ATOMIC: two roots racing one scope otherwise both read no standing, both register, and both
        # bind, the loser's stream orphaned with the receipt of a plane nothing drains.
        with cls._installing_lock(scope):
            with cls._gate:
                standing = cls._receipts.try_find(scope)
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
        # get-or-mint under the custody gate, then released before the returned lock is taken, so the two locks are
        # never held in one order and the process gate is never the one an install holds across `Hooks.register`.
        with cls._gate:
            held = cls._installing.try_find(scope).default_with(RLock)
            cls._installing = cls._installing.add(scope, held)
        return held

    @classmethod
    def _registered(cls, scope: ScopeKey) -> RuntimeRail[Block[HookPoint[Struct]]]:
        # point ids are composition-unique and the registry ships no retirement, so registration latches per scope
        # while ledger custody re-arms freely: a re-install after `closed` must not re-register ids it already
        # owns, where a claim over the duplicate would refuse the whole restart.
        # ONE roster claim: the registry's whole-set arm swaps its point table only past the last admitted row, so a
        # refused install leaves custody exactly as it stood and this latch can never mark a scope pointed against a
        # half-mounted roster — the accumulating diagnosis a per-point traverse bought by surrendering that atomicity.
        return (
            Ok(Block.empty()) if scope in cls._pointed else Hooks.register(POINTS, scope=scope).map(lambda points: cls._latched(scope, points))
        )

    @classmethod
    def _latched(cls, scope: ScopeKey, points: Block[HookPoint[Struct]]) -> Block[HookPoint[Struct]]:
        with cls._gate:
            cls._pointed = cls._pointed | {scope}
        return points

    @classmethod
    def _bind(cls, ledger: Ledger, custody: Custody, service: str, scope: ScopeKey) -> JournalReceipt:
        # deposit lands OUTSIDE this owner's gate: the hooks ledger takes its own lock and passes the receipt
        # through, so the bundle reads one durable-plane answer and no two-lock ordering exists to invert. The
        # stream roster derives from the family rather than a hand-listed pair, so a third record cannot leave this
        # receipt advertising a vocabulary the plane outgrew.
        send, receive = anyio.create_memory_object_stream[Fact](max_buffer_size=INTAKE)
        receipt = JournalReceipt(ledger=type(ledger).__qualname__, service=service, streams=STREAMS, classes=tuple(Retain))
        with cls._gate:
            cls._bound = cls._bound.add(scope, Bound(ledger=ledger, custody=custody, service=service))
            cls._intake, cls._drain = cls._intake.add(scope, send), cls._drain.add(scope, receive)
            cls._receipts = cls._receipts.add(scope, receipt)
            cls._retired = cls._retired - {scope}
        return Hooks.installed(OWNER, receipt, scope=scope)

    @classmethod
    def bound(cls, scope: ScopeKey = DEFAULT_SCOPE) -> RuntimeRail[Bound]:
        return cls._bound.try_find(scope).to_result_with(lambda: BoundaryFault(config=("journal.ledger", "ledger unbound")))

    @classmethod
    async def record(
        cls, source: Recordable, *, causal: Option[CausalFrame] = Nothing, scope: ScopeKey = DEFAULT_SCOPE
    ) -> RuntimeRail[int]:
        # one polymorphic writer discriminating on the value, so arity never reaches the signature, and the ONE
        # stamp authority — a caller-threaded stamp lets two producers mint one content key and dedup genuine
        # evidence away as a redelivery. `causal` is the inbound half a serve leg or worker crossing already holds,
        # so a fact orders strictly after the cause its caller saw; the send AWAITS, so a full intake stalls its
        # producer rather than dropping a row nothing replays. An EMPTY iterable rides that same fold — a metering
        # fan that priced nothing offers no fact — so it gates nothing, ticks no stamp, and answers `Ok(0)`; a
        # width test short-circuiting it AHEAD of the intake match would delete the retirement fault for exactly the
        # legs that record only when they charge, which is where a producer outliving its plane goes unseen longest.
        offered = Block.of_seq((source,) if isinstance(source, AuditFact | MeterFact) else source)
        # binding order is load-bearing: the intake resolves FIRST, so a scope with no live plane settles without
        # firing an admission policy over facts nothing accepts, and the two absent states settle apart. RETIRED is
        # a fault — this producer outlived the plane it writes to. UNARMED is a lawful no-op answering the count it
        # landed: no composition ever installed here, which is a deployment declaration that this process journals
        # nothing, so a producer binds one rail either way and pays one map read where evidence was never deployed.
        # Nothing is recorded on that path — a drop counter prices the hot path of a composition that asked for no
        # plane, and the census proving this owner's measures never ran on a scope no install crossed.
        match cls._intake.try_find(scope):
            case Option(tag="some", some=send):
                # the stamp is the LAST step and only on the admitted path, since the cell it ticks is process-wide:
                # stamping ahead of the gates advances this node's successor for every refused batch, so a vetoing
                # subscriber leaves gaps in the one coordinate the stream sorts on, and the gate reads each record's
                # default `UNSTAMPED` slot exactly as its declaration promises. Tenancy resolves BEFORE the gate, so
                # a subscriber judges the row the plane will store rather than the caller's unattributed draft.
                attributed = Block.of_seq(_tenanted(fact) for fact in offered)
                match cls._admitted(attributed, scope):
                    case Result(tag="ok"):
                        return await cls._offered(send, attributed.map(lambda fact: replace(fact, stamp=cls.stamped(causal))))
                    case refused:
                        return Error(refused.error)
            case _ if scope in cls._retired:
                return Error(BoundaryFault(config=("journal.record", "journal custody retired")))
            case _:
                return Ok(0)

    @staticmethod
    def _admitted(facts: Block[Fact], scope: ScopeKey) -> RuntimeRail[Block[Fact]]:
        # append gating is a VETO seam run BEFORE a fact occupies intake capacity, so a refused fact costs no drain
        # cycle and its refusal reaches the caller as that caller's own rail; each record names its OWN point through
        # `gate`, so an ungated stream skips the fold by declaration and a newly gated stream is one ClassVar beside
        # one `POINTS` row. The admitted payload is READ FOR ITS DISPOSITION ALONE — evidence with two authors is no
        # longer evidence — and abort disposition refuses the whole batch, since partial admission lands rejected
        # evidence with no coordinate naming what survived.
        return traversed(
            facts.choose(lambda fact: Option.of_optional(fact.gate).map(lambda point: Hooks.fire(point, fact, scope=scope))),
            by=Disposition.ABORT,
        )

    @staticmethod
    async def _offered(send: MemoryObjectSendStream[Fact], facts: Block[Fact]) -> RuntimeRail[int]:
        async def suspended() -> int:
            for fact in facts:  # Exemption: the suspending send is the never-shed seam — no expression form awaits
                await send.send(fact)
            return len(facts)

        return await async_boundary("journal.offer", suspended)

    @classmethod
    async def drained(
        cls, *, scope: ScopeKey = DEFAULT_SCOPE, task_status: TaskStatus[None] = TASK_STATUS_IGNORED
    ) -> RuntimeRail[JournalDrain]:
        # single-consumer drain a composition root starts inside its own task group — `tg.start` blocks on the
        # readiness signal, so no producer suspends into an intake nothing reads yet. Taking the receive handle out
        # of custody makes the single consumer structural: a second concurrent drain on one scope finds nothing and
        # refuses instead of silently splitting the stream across two tallies. The tally returns once intake closes
        # and the last window lands — what makes `closed` a shutdown rather than a truncation — and the root reads
        # it off the child handle's `return_value` after the group closes.
        with cls._gate:
            taken = cls._drain.try_find(scope)
            cls._drain = cls._drain.remove(scope)
        match (taken, cls.bound(scope)):
            case (Option(tag="some", some=receive), Result(tag="ok", ok=held)):
                settled = JournalDrain()
                task_status.started()
                async with receive:
                    # intake depth plus suspended senders registers as the standing occupancy level under this
                    # owner's own band, so a full intake with a healthy ledger is visible pressure the lane and
                    # pool bands never absorb — the deferral counter sees only the ledger.
                    with Metrics.occupied(
                        lambda: (depth := receive.statistics()).current_buffer_used + depth.tasks_waiting_send,
                        band=OWNER,
                        scope=scope,
                    ):
                        try:
                            async for batch in _batched(receive):  # Exemption: the drain threads its own tally; no expression form awaits
                                settled = JournalDrain.combined(settled, await cls._landed(held, batch, scope))
                        finally:
                            # terminal work runs on EVERY exit, cancellation included, so the batch in flight
                            # joins the facts still buffered behind it in one roster; empty on the clean path.
                            cls._owed(scope, _swept(receive, held.service))
                return Ok(settled)
            case _:
                return Error(BoundaryFault(config=("journal.drain", "ledger unbound or drain already owned")))

    @classmethod
    def closed(cls, *, scope: ScopeKey = DEFAULT_SCOPE) -> RuntimeRail[None]:
        # graceful shutdown IS closing the intake: every buffered fact still delivers, the partial window flushes,
        # and `drained` returns clean. Custody RETIRES with the close and the scope MARKS retired, so a later
        # `install` re-arms it instead of adopting a closed stream every `record` then rails `resource` against, and
        # a fact offered meanwhile refuses as the fault it is rather than folding into the never-installed no-op;
        # unlanded rows and the point latch survive that retirement, because a shutdown owing rows is exactly when a caller reads them
        # and the registry retires no id. Stays synchronous so a signal handler, an atexit hook, or a non-async
        # `finally` reaches it with no loop. One gate pass decides ownership of the receive end against `drained`'s
        # take, so exactly one of the two ever holds it and the close arm is LOSSLESS: a scope closed before any
        # drain started still owns a buffered window, which sweeps into the unlanded roster here rather than dying
        # with the dropped handle on the one plane that refuses to shed.
        with cls._gate:
            held, taken = cls._intake.try_find(scope), cls._drain.try_find(scope)
            service = cls._bound.try_find(scope).map(lambda bound: bound.service)
            cls._intake, cls._drain = cls._intake.remove(scope), cls._drain.remove(scope)
            cls._bound, cls._receipts = cls._bound.remove(scope), cls._receipts.remove(scope)
            cls._retired = cls._retired | {scope}
        taken.bind(lambda receive: service.map(lambda name: cls._stranded(scope, receive, name)))
        return held.to_result_with(lambda: BoundaryFault(config=("journal.close", "ledger unbound"))).map(lambda send: send.close())

    @classmethod
    def _stranded(cls, scope: ScopeKey, receive: MemoryObjectReceiveStream[Fact], service: str) -> None:
        # close-before-drain: nothing ever took this handle, so its buffer and every sender suspended behind it
        # sweep into the unlanded roster before the handle closes. `_swept` is checkpoint-free, which is what lets
        # the synchronous close run it, and the roster is where a caller re-offers or persists what never landed.
        cls._owed(scope, _swept(receive, service))
        receive.close()

    @classmethod
    def pending(cls, *, scope: ScopeKey = DEFAULT_SCOPE) -> Block[FactRow]:
        # rows a drain took from the intake and never landed — empty on every clean path, and the ONE surface a
        # bounded shutdown reads: a root capping the wait with its own `CancelScope` gets no return value from a
        # tripped scope, so the rows a dead ledger still owes leave HERE as data a caller re-offers or persists.
        with cls._gate:
            return cls._pending.try_find(scope).default_value(Block.empty())

    @classmethod
    def _owed(cls, scope: ScopeKey, rows: Block[FactRow]) -> None:
        # ACCUMULATES: a scope that shut down owing rows keeps them across the re-install `closed` invites, so the
        # next drain's first batch cannot overwrite a prior session's unlanded roster. Replacing the slot instead
        # sheds exactly the evidence `pending` exists to make visible, on the one plane that refuses to shed.
        with cls._gate:
            cls._pending = cls._pending.add(scope, cls._pending.try_find(scope).default_value(Block.empty()).append(rows))

    @classmethod
    def _settled(cls, scope: ScopeKey, rows: Block[FactRow]) -> None:
        # Removes exactly the keys this landing covered rather than emptying the slot, so a landing never clears rows
        # a prior session still owes; the content key is the identity both sides already agree on.
        landed = frozenset(row.key for row in rows)
        with cls._gate:
            cls._pending = cls._pending.add(
                scope, cls._pending.try_find(scope).default_value(Block.empty()).filter(lambda row: row.key not in landed)
            )

    @classmethod
    async def _landed(cls, held: Bound, batch: Block[Fact], scope: ScopeKey) -> JournalDrain:
        # fact and row travel PAIRED, because the projection reads the fact and the landing answers in keys: a
        # batch replayed after a lost acknowledgement lands entirely as duplicates, and projecting off `batch`
        # instead would charge that customer twice and write the audit line twice on the one plane that exists
        # to be evidence.
        paired = batch.map(lambda fact: (fact, _rowed(fact, held.service)))
        rows = paired.map(lambda pair: pair[1])
        cls._owed(scope, rows)
        delay = BACKOFF_SECONDS
        # `stamina` stays refused here by settled law: its schedule rejects the unbounded `attempts=None` +
        # `timeout=None` pair the never-shed law requires, and it retries raised transients where this port
        # returns railed verdicts the loop must read.
        while True:  # Exemption: unbounded retry is the never-shed law — a dead ledger suspends, never sheds
            match await async_boundary("journal.append", lambda: held.ledger.landed(rows)):
                # a landing accounts for every offered row or it is not a landing: a short write retries, and the
                # retry is safe precisely because the content key dedups whatever already landed.
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
    async def groomed(cls, *, scope: ScopeKey = DEFAULT_SCOPE) -> RuntimeRail[Groomed]:
        # aging is an ordinary reclaim the ledger's own mechanism executes — table TTL, partition drop, a scheduled
        # maintenance statement — since no telemetry worker or scheduler surface enters this branch for retention.
        # Cutoffs derive from THIS owner's clock, never a caller instant, so cutoff and stamp share one time base,
        # and a sealed field becomes unreadable through key destruction alone, never through a groomed row.
        async def swept(held: Bound) -> RuntimeRail[Groomed]:
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

```python signature
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
        # This fold seats the rollup monoid WITH its own identity and operator, and answers an implementer whose
        # engine carries no grouped read: `tallied` folds it rather than re-deriving a billing rollup beside a
        # ledger, so pushed-down and row-wise arms return one algebra. Audit rows drop HERE rather than at the read,
        # so one scan surface serves a subject export and a billing period with neither re-filtering the other's
        # rows. One AVL descent per row: `Map.change` fuses lookup and insert on the hottest path a settlement takes
        # over a month of rows, an absent key seeding on `ZERO`.
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
    # rollup key ordered by construction for the `Map` tree: `attributed` ranks unattributed rows first with `tenant`
    # "" only beneath it, so the order stays TOTAL on a mixed-tenancy window and `held` alone lifts the pair back
    # into absence.
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

# monoid identity every fold seeds on and every absent key defaults to, so a zero row is the algebra's own unit
# rather than a literal re-minted at each fold site.
ZERO: Final[Aggregate] = Aggregate(count=0, total=0)

# --- [OPERATIONS] -----------------------------------------------------------------------


async def rolled(source: Block[Fact] | Scan, *, scope: ScopeKey = DEFAULT_SCOPE) -> RuntimeRail[Billed]:
    # one entry over two input shapes: rows in hand fold through the monoid's own `Aggregate.rolled`, a scan
    # coordinate hands the group-by to the bound ledger, whose engine already indexes the window over the
    # `(tenant, resource, quantity)` columns `FactRow` lifts. Both arms answer one algebra, so a caller never learns
    # which side grouped.
    match source:
        case Scan() as coordinate:

            async def tallied(held: Bound) -> RuntimeRail[Billed]:
                return await held.ledger.tallied(coordinate)

            return await _resolved(scope, tallied)
        case facts:
            return Ok(Aggregate.rolled(facts))


def _priced(key: Priced, aggregate: Aggregate, rating: Rating) -> RuntimeRail[tuple[Priced, Charge]]:
    # the money context's armed traps RAISE — an invalid, overflowing, or float-touched rate is a `decimal`
    # exception, never a value — so the whole charge expression runs INSIDE the rating fence. Computed under
    # `map` instead it escapes the carrier entirely and takes the settlement traversal down with it, which is
    # exactly the refusal the traps were armed to make visible rather than silent.
    return rating.try_find(key.resource).to_result_with(lambda: BoundaryFault(config=("journal.rate", key.resource.value))).bind(
        lambda rate: boundary(
            "journal.rate",
            lambda: (key, Charge(amount=MONEY.quantize(MONEY.multiply(Decimal(aggregate.total), rate.per), SCALE), currency=rate.currency)),
            catch=DecimalException,
        )
    )


def rated(billed: Billed, rating: Rating) -> RuntimeRail[Map[Priced, Charge]]:
    # exact end to end: the integral total lifts through `Decimal(int)` losslessly, multiplies inside the money
    # context whose traps refuse an invalid, overflowing, or float-touched rate, and quantizes half-even at scale
    # four exactly once at the terminal — a per-row round accumulates a drift a settlement cannot reconcile
    # against its own aggregate. `traverse` threads the rows, aborting on the first unrated resource by name.
    return traverse(lambda row: _priced(row[0], row[1], rating), Block.of_seq(billed.items())).map(Map.of_seq)
```

## [05]-[SHREDDER]

- Owner: `SubjectKey` is the tenant-scoped custody identity, the ledger holds one wrapped data key per identity, and `sealed`/`opened`/`erased` compose the envelope algebra over it. Custody stores the WRAPPED form alone — raw data keys never cross the ledger seam — so a posture changes by swapping a `Custody` value and this page never learns which holder issued the wrap; the custody arms are async whole exactly as the ledger is, so a remote KMS arm binds as an ordinary instance rather than an on-loop stall, and both wrap under deterministic AAD-bound AES-SIV, so the KEK path carries no nonce custody to misuse. Two instances ship: `local` holds material a root already resolved, and `vault` reads the deployment's own credential ladder through the settings-admitted secret boundary on EVERY call, so a rotated KEK reaches the next wrap with no rebind and an unnamed one refuses instead of minting a substitute that would leave every prior envelope permanently unopenable.
- Cases: `opened` is TOTAL over erasure — a destroyed or absent key folds to `Nothing`, which every reader renders through the receipts-owned redaction marker, because erasure is a lawful state and never an error to recover from. `InvalidTag` on LIVE key material stays a fault on the rail: tampering and erasure are different facts, and folding a tag failure to absence renders a corrupted payload as a lawfully erased one.
- Entry: `sealed` claims atomically — a fresh mint inserts, a concurrent or replayed subject keeps the stored wrapped key and the loser seals under the winner by unwrapping the returned row, and a destroyed subject resurrects under a NEW key so every envelope written before the erasure stays unreadable forever. Claiming before sealing is load-bearing: two recorders racing one subject otherwise seal under two data keys, and destroying either leaves half that subject's evidence readable.
- Entry: `exported` is the portability read — one `Scan.subject` over the same index every append wrote and every erasure keys on, so a data-subject request is an index scan rather than a stream crawl. Sealed fields inside a payload stay sealed: field shapes are application material, so the exporting consumer composes `opened`/`redacted` per field it knows, and an erased subject's fields render the marker rather than failing the export.
- Auto: every envelope binds to its `SubjectKey` through the associated-data slot, so a ciphertext lifted onto another subject or another tenant fails its authentication tag rather than opening under a key that happens to be live. Every AEAD call crosses one native fence, so an unbuildable `cryptography` classifies `import_` on the rail wherever the process first touches it — a read-only replica that never seals included.
- Auto: `erased` mints the tombstone under the writer's own stamp — a ledger supplies neither order nor identity, so it only empties the custody slot and echoes the stone it persisted — records it as a regulatory audit fact carrying the erased subject in its own index, and fans the observe point through the async mirror so an async compliance observer is reachable. Destruction is irreversible, so that record's rail BINDS into the erasure verdict: a refused or unlanded tombstone surfaces naming the subject whose key is already gone, where a dropped rail leaves an erasure no export can evidence.
- Growth: a new custody posture is one `Custody` instance the composition root binds beside the shipped `local` and `vault` pair — a hardware holder or a cloud KMS arm lands as one more value, never a field on either; a new secret backend behind `vault` is a `CloudVault` arm at the settings owner with zero edit here; a new sealed field is a caller-side projection composing `sealed`/`opened`, since field shapes are application material; a new export surface is one projection over the same `Export`, never a second subject read.
- Boundary: erasure destroys key material and touches no row — the append-only invariant survives the right to erasure because unreadable IS erased — and the export and the erasure prove one spine: export reads what remains readable, erasure makes fields unreadable, and the stored bytes stay untouched either way.

```python signature
# --- [MODELS] ---------------------------------------------------------------------------


class Export(Struct, frozen=True):
    # portability projection carrying the identity it answers for, so an export document names its own subject
    # rather than leaving the caller to re-pair a loose fact block with the request that produced it.
    subject: SubjectKey
    facts: Block[Fact]


# --- [OPERATIONS] -----------------------------------------------------------------------


async def _cipher(data_key: bytes, plain: bytes, key: SubjectKey) -> RuntimeRail[Sealed]:
    # AES-GCM-SIV seals the envelope under a random nonce: equal payloads stay unlinkable — the deterministic SIV
    # arm would hand an equality oracle over subject material — and a repeated nonce degrades to that equality
    # leak alone instead of GCM's key-stream break.
    nonce = token_bytes(NONCE_BYTES)
    return await _offloaded("journal.seal", lambda: Sealed(nonce=nonce, cipher=AESGCMSIV(data_key).encrypt(nonce, plain, key.aad)))


async def sealed(plain: bytes, key: SubjectKey, *, scope: ScopeKey = DEFAULT_SCOPE) -> RuntimeRail[Sealed]:
    async def claimed(held: Bound) -> RuntimeRail[Sealed]:
        # Minting names the primitive that CONSUMES the key, so a width the envelope cipher refuses cannot pass a
        # sibling primitive's own generator and fail later at the seal.
        offered = await _chained(
            _crypto("journal.mint", lambda: AESGCMSIV.generate_key(bit_length=KEY_BITS)), lambda fresh: held.custody.wrap(fresh, key.aad)
        )
        winner = await _chained(offered, lambda material: held.ledger.claimed(key, material))
        data_key = await _chained(winner, lambda material: held.custody.unwrap(material, key.aad))
        return await _chained(data_key, lambda opened_key: _cipher(opened_key, plain, key))

    return await _resolved(scope, claimed)


async def opened(envelope: Sealed, key: SubjectKey, *, scope: ScopeKey = DEFAULT_SCOPE) -> RuntimeRail[Option[bytes]]:
    # total over erasure: an absent or destroyed key answers `Nothing`, the lawful state a reader folds to its
    # redaction marker; a live key whose tag fails stays an `Error`, since the fence classifies `InvalidTag` onto
    # that boundary rail and a tampered payload never renders as a lawfully erased one.
    async def unsealed(held: Bound) -> RuntimeRail[Option[bytes]]:
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


async def exported(key: SubjectKey, *, scope: ScopeKey = DEFAULT_SCOPE) -> RuntimeRail[Export]:
    async def scanned(held: Bound) -> RuntimeRail[Export]:
        return (await held.ledger.scanned(Scan(subject=key))).map(lambda facts: Export(subject=key, facts=facts))

    return await _resolved(scope, scanned)


def redacted(held: Option[bytes]) -> bytes:
    # one erased-state render for every reader: the receipts-owned marker, so a redacted field reads identically in
    # a log line, an export document, and a projection row.
    return held.default_value(REDACTED.encode())


def _erasure(stone: Tombstone) -> AuditFact:
    # stamp stays unset here: this fact takes its admission coordinate from `record` like every other, while the stone
    # it reports already took its own at the instant the key died — two facts, two coordinates, one process cell.
    return AuditFact(
        action="subject.erased",
        actor=Party(kind=Actor.SYSTEM, key="retain"),
        target=Party(kind="subject", key=stone.subject),
        retention=Retain.REGULATORY,
        change=(Cleared(path="/key", prior="wrapped"),),
        subjects=(stone.subject,),
        tenant=stone.tenant,
    )


async def erased(key: SubjectKey, *, scope: ScopeKey = DEFAULT_SCOPE) -> RuntimeRail[Option[Tombstone]]:
    # destruction IS the erasure verb: THIS writer mints the stone — a ledger mints neither stamp nor identity — and
    # its ledger empties the custody slot and echoes what it persisted, the tombstone lands as regulatory journal
    # truth, and the observe point fans through the async mirror so a tap returning an awaitable is awaited. The
    # record's rail BINDS into this verdict because destruction is irreversible — a refused or unlanded tombstone
    # surfaces naming the subject whose key is already gone. `Nothing` means no live key existed: nothing destroyed,
    # nothing fans, nothing records.
    async def destroyed(held: Bound) -> RuntimeRail[Option[Tombstone]]:
        stone = Tombstone(subject=key.subject, tenant=key.tenant, destroyed=Journal.stamped())
        match await held.ledger.destroyed(stone):
            case Result(tag="ok", ok=Option(tag="some", some=persisted)):
                await Hooks.fire_async(ERASE_POINT, persisted, scope=scope)
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

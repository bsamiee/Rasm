# [PY_RUNTIME_EVENT]

Branch-wide CloudEvents ownership seats here: `MessageEnvelope` is the frozen attribute algebra every Python producer mints and every consumer decodes, `EventType`/`Source`/`OperationId` close the attribute grammar the specification fixes, `Extensions` is the typed roster handed at construction and at every decode, and `EventFormat` closes the format contract over the specification's structured, binary, and batch content modes, each a capability column rather than a shape every format is assumed to carry. Message envelopes ANNOUNCE a fact and gain no authority over it — the producing receipt stays the evidence truth and the message envelope projects it — so a consumer routes on attributes without opening the payload. Transport lowering, protocol settings, and payload residence seat at `transport/binding#BINDING`; this page stops at attribute bytes.

Specification law owns every row and `cloudevents` accelerates it: `core.v1.event.CloudEvent` is the one admitted event class and its aggregating `CloudEventValidationError` funnels through one `boundary` fence into `BoundaryFault`, while the whole `cloudevents.v1` legacy tree — its mutable dict event, its converter stack, its marshaller pair, and its pydantic mirror — is refused by ruling. Where the package's surface is narrower than the specification the branch owns the remainder outright: the extension-name ceiling, the whole batch leg, and every format past JSON. Rails, faults, and the traversal dispositions arrive settled from `reliability/faults#FAULT`; the capability-subject roster from `observability/metrics#METRIC`; `Classification` and the W3C `Correlation` fold from `execution/admission#CONTEXT`; `ContentKey` from `evidence/identity#IDENTITY`; `Hlc` from `evidence/clock#CLOCK`.

## [01]-[INDEX]

- [02]-[GRAMMAR]: attribute grammar — `EventType` segments against the capability-subject roster, `Source`, `OperationId`, the uniqueness composite, and the extension-name ceiling the package does not carry.
- [03]-[MESSAGE]: `MessageEnvelope` — the mint boundary, the typed `Extensions` roster, the two-trace split, the `dataref` join, and the DSSE attribute digest.
- [04]-[FORMAT]: `EventFormat` — the protocol members, the format roster with its two capability columns, the binary-mode payload seam, and the branch-owned batch leg.

## [02]-[GRAMMAR]

- Owner: `EventType` is the parsed `rasm.<domain>.<subject>.<fact>.v<N>` value and the ONE site that spells the pattern, so a producer names a fact by constructing the value rather than formatting a string a subscription later fails to match. `Source` and `OperationId` are its siblings on the required triple, each a refinement over its own domain — `Source` a URI-reference naming the producing CAPABILITY, `OperationId` the producer's operation identity — and `Uniqueness` is the `(source, id)` composite every dedup window and idempotency key reads as one value rather than joining two fields at each consumer.
- Cases: `<domain>` proves against `observability/metrics#METRIC` `DOMAINS` at construction, so a board and a subscription join ONE capability vocabulary and a fact under an unrostered segment refuses exactly as the metric under that segment refuses — the join is the point, and a second event-only segment roster is the fork it forecloses. `<subject>` is the capability's own noun, `<fact>` reads past tense, and `<version>` moves only on a breaking `dataschema` change so a compatible widening leaves every standing subscription matching.
- Law: `id` is operation identity and never a content digest — a producer replaying one operation replays one `id`, and the payload's identity rides `subject` under `evidence/identity#IDENTITY`'s `wire` render, the same spelling `dataref` publishes where the payload externalizes. Collapsing the two erases the exactly-once question: two operations over identical bytes are two facts a consumer must see, and one operation retried is one fact it must not.
- Law: `source` names the producing capability and never a host, package, process, or deployment, because a redeployment re-authors the identity every subscription keyed on. It is a producer CLAIM verified against the trust row before any routing decision reads it, exactly as `authcontext` is, so neither reaches a filter unverified.
- Law: extension-name ceilings are BRANCH-owned. `core.v1.event.CloudEvent._validate_extension_attributes` matches `^[a-z0-9]+$` over a one-character floor and rejects the reserved `data` name — a charset rule carrying no length bound — so `EXTENSION_FMT` states the specification's twenty-character ceiling and the mint proves it BEFORE construction. Package surfaces narrower than the specification state a fact about the package, never a ceiling on what the estate carries.
- Law: an inbound extension name the roster does not hold, or one past the ceiling, is IGNORED and never a whole-message fault, so a peer's private extension never sheds the fact it rode in on; the ignored set surfaces as decode evidence rather than silence.
- Law: `TYPE_GRAMMAR` is the ONE compiled spelling and both entries cross it — `parse` reads it and `of` renders `_spelled` into it — so a mint proves exactly what a decode proves and neither restates the other's character class. Bare-name sequence arms (`case [TYPE_STEM, domain, ...]`) CAPTURE rather than compare: such an arm rebinds the constant it appears to read and admits every five-segment spelling, so a compiled grammar is what refuses a foreign stem at all and what proves `<subject>` and `<fact>` a producer hands in loose.
- Law: the stamp pair leaves as the MEASURED lag rather than as the two stamps a caller already holds — `stamped` is `Announced.lag`'s one producer, so a receipt never publishes a zero no observation took, under `docs/laws/scars.md` `[FORGED_ZERO]`.
- Law: every refusal resolves ONE `reliability/faults#FAULT` `RAISES` anchor under `RuntimeLeg.EVENT` and derives its subject from that leg, so a fence spells no coordinate its package never declared and the closed `defect` token plus the row's NAMED slots replace the sentences the literal constructions carried. One `EVENT_EXTENSION` anchor serves all four extension codecs: the refused spelling a peer repairs on rides the caught class the fence names, so the per-codec subject bought nothing the detail did not already hold.
- Entry: `EventType.of(domain, subject, fact, version)` is the one mint and `EventType.parse(spelling)` the one admission it composes, both railed, so the wire spelling round-trips through one owner. `Uniqueness.of(envelope)` projects the composite off a decoded message rather than taking two loose arguments a caller can transpose.
- Growth: a new capability subject is one `DOMAINS` row at the metrics owner, reaching this grammar untouched; a new fact under a standing subject is a `<fact>` value and no declaration at all; a breaking payload change is one `<version>` increment beside its `dataschema` move.
- Boundary: attribute grammar only — no transport header spelling, no filter dialect, no subscription. Rejected: a literal `BoundaryFault(...)` construction beside a rostered anchor; a hand-formatted `f"rasm.{...}"` type string beside this owner; a bare-name sequence pattern standing in for a stem comparison; a segment admitted by the mint that the grammar refuses; an event-local capability-segment roster; a `subject` spelling that is not `ContentKey.project("wire")`; a content digest in `id`.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import re
from datetime import datetime
from typing import Annotated, Final, Self
from urllib.parse import urlsplit

from expression import Error, Ok
from msgspec import Meta, Struct

from rasm.runtime.faults import EVENT_DOMAIN, EVENT_LAG, EVENT_NAIVE, EVENT_SOURCE, EVENT_TYPE, RuntimeRail
from rasm.runtime.metrics import DOMAINS

# --- [TYPES] ----------------------------------------------------------------------------

# aliases evaluate lazily, so each refines against the ONE grammar below rather than re-spelling its character
# class: `Fact` reads past tense by a convention the mint cannot enforce, while `Version` is the breaking-change
# ordinal `dataschema` moves with rather than beside.
type Segment = Annotated[str, Meta(pattern=rf"^{SEGMENT}$")]
type Version = Annotated[int, Meta(ge=1)]
# content key AS IT CROSSES: the bare 32-lowercase-hex `ContentKey.project("wire")` render every branch publishes.
# It is the message envelope's own slot type because the pair must round-trip — a `ContentKey` slot needs `fmt` and
# `byte_length`, and the pinned spelling carries neither, so a decode rebuilding one fabricates both.
type WireKey = Annotated[str, Meta(pattern=r"^[0-9a-f]{32}$")]

# --- [CONSTANTS] ------------------------------------------------------------------------

# ONE compiled grammar over the whole spelling — stem, three segments, and the version ordinal — that `parse`
# reads and `of` renders against, so neither entry restates the other's rule and a segment a producer names is
# proven by the same pattern a decode runs. A `case [TYPE_STEM, domain, ...]` sequence arm CAPTURES the bare name
# rather than comparing it, admitting every five-segment spelling while rebinding the constant it appears to read,
# so the compiled form is what refuses a foreign stem at all.
SEGMENT: Final[str] = r"[a-z0-9]+(?:-[a-z0-9]+)*"
TYPE_STEM: Final[str] = "rasm"
TYPE_GRAMMAR: Final[re.Pattern[str]] = re.compile(
    rf"{TYPE_STEM}\.(?P<domain>{SEGMENT})\.(?P<subject>{SEGMENT})\.(?P<fact>{SEGMENT})\.v(?P<version>[1-9][0-9]*)"
)
# branch law over package law: `CloudEvent` proves the CHARSET and the one-character floor, this states the
# specification's twenty-character ceiling the package never reads, so the mint refuses a name `CloudEvent` admits.
EXTENSION_FMT: Final[re.Pattern[str]] = re.compile(r"^[a-z0-9]{1,20}$")

# --- [MODELS] ---------------------------------------------------------------------------


class EventType(Struct, frozen=True, order=True, gc=False):
    # ONE site spells the type grammar. `order=True` because a subscription roster is an ordered `Map` keyed
    # on this value, and a second `f"rasm.{domain}..."` anywhere forks what every filter's prefix dialect matches.
    domain: Segment
    subject: Segment
    fact: Segment
    version: Version

    @classmethod
    def of(cls, domain: str, subject: str, fact: str, version: int, /) -> RuntimeRail[Self]:
        # `of` RENDERS through the spelling `wire` publishes and ADMITS through the grammar `parse` reads, so a
        # producer cannot construct a value the wire round-trip then refuses and no segment reaches the wire unproven.
        return cls.parse(_spelled(domain, subject, fact, version))

    @classmethod
    def parse(cls, spelling: str, /) -> RuntimeRail[Self]:
        # `<domain>` proves against the METRIC roster, never a local set: one capability vocabulary means a board
        # and a subscription name the same thing, and an unrostered segment is a missing capability row upstream.
        return (
            Error(EVENT_TYPE.raised(spelling))
            if (found := TYPE_GRAMMAR.fullmatch(spelling)) is None
            else Error(EVENT_DOMAIN.raised(found["domain"]))
            if found["domain"] not in DOMAINS
            else Ok(cls(domain=found["domain"], subject=found["subject"], fact=found["fact"], version=int(found["version"])))
        )

    @property
    def wire(self) -> str:
        return _spelled(self.domain, self.subject, self.fact, self.version)


class Source(Struct, frozen=True, order=True, gc=False):
    # a URI-reference naming the producing CAPABILITY. Relative is lawful by specification and load-bearing here:
    # an absolute form pins a host, and a redeployment behind the same capability re-authors the identity every
    # standing subscription keyed on. The value is a producer CLAIM the trust row verifies before any filter reads it.
    reference: Annotated[str, Meta(min_length=1)]

    @classmethod
    def of(cls, reference: str, /) -> RuntimeRail[Self]:
        # `urlsplit` is total over a relative reference, so the gate is on the parts a capability reference may not
        # carry — a query and a fragment are routing state, never identity.
        return (
            Error(EVENT_SOURCE.raised())
            if (parts := urlsplit(reference)).query or parts.fragment
            else Ok(cls(reference=reference))
        )


class OperationId(Struct, frozen=True, order=True, gc=False):
    # producer OPERATION identity — a retried operation replays this value and two operations over identical
    # bytes never share it. Content identity is `subject`'s, so this slot never takes a digest render.
    value: Annotated[str, Meta(min_length=1)]


class Uniqueness(Struct, frozen=True, order=True, gc=False):
    # `(source, id)` is the composite the specification fixes: every dedup window, idempotency key, and matched-duplicate
    # half reads THIS value, so no consumer joins two loose fields and none keys on `id` alone across sources.
    source: Source
    operation: OperationId

    @classmethod
    def of(cls, envelope: "MessageEnvelope", /) -> Self:
        # projected off the decoded owner rather than two loose arguments a caller transposes, so the composite a
        # producer keys its dedup on and the one a settlement reads are the same value built at one site.
        return cls(source=envelope.source, operation=envelope.operation)


# --- [OPERATIONS] -----------------------------------------------------------------------


def _spelled(domain: str, subject: str, fact: str, version: int, /) -> str:
    # `_spelled` IS the ONE render the mint and the `wire` view both compose, so the grammar has one producer beside its
    # one admission and a second `f"rasm.{...}"` anywhere is the fork every filter's prefix dialect then finds.
    return f"{TYPE_STEM}.{domain}.{subject}.{fact}.v{version}"


def stamped(occurred: datetime, recorded: datetime, /) -> RuntimeRail[float]:
    # `time` mints at the producer and `recordedtime` at the receiver, so the pair MEASURES the queue collapsing them
    # erases — and the measurement is what leaves, filling `Announced.lag` from the one site that proved it rather
    # than a zero a later reader cannot tell from a real reading. A stamp after its own observation leaves the lag
    # unmeasured and refuses rather than publishing a negative one.
    return (
        Error(EVENT_NAIVE.raised())
        if occurred.tzinfo is None or recorded.tzinfo is None
        else Error(EVENT_LAG.raised())
        if recorded < occurred
        else Ok((recorded - occurred).total_seconds())
    )
```

## [03]-[MESSAGE]

- Owner: `MessageEnvelope` is the frozen canonical owner — the required triple, the optional four, the typed `Extensions` roster, and the payload as one `Raw` band — and `Extensions` is the roster spelled ONCE for the whole branch, handed at construction and at every decode because a decoder without it reads a declared extension as an unknown string. `EXTENSION_ROWS` is the table both directions fold: each row binds a member — whose value IS both its wire name and its slot name — to the `Codec` its carried type answers, so a seventeenth extension is one member, one typed slot, and one row while neither projection is edited and no two rows repeat a body between them.
- Cases: `Extensions` slots are `Option`-typed and each carries its own domain rather than a string — `partitionkey` a `str`, `sequence` an `int` beside `sequencetype`'s vocabulary, `sampledrate` a positive denominator, `dataref` a `WireKey`, `dataclassification` the `execution/admission#CONTEXT` `Classification` grade family — `public`, `internal`, `restricted`, `secret`, ascending, transcribed meaning-identical from the estate seam and closed at that one owner, its per-grade redaction transform and binding reach riding `transport/binding#BINDING`'s own row table — `recordedtime`/`expirytime` aware `datetime`s, `severity` its own grade, `correlation` a causal chain id, `deprecation` a superseding `EventType` beside its window, `authcontext` the producer's asserted principal, `dssematerial` the signed bytes. Passthrough `frozendict[str, str]` bands beside them are the deleted form: they mint spec-invalid names at no seam and erase every value type a consumer then re-parses.
- Law: the mint is the ONE boundary. `MessageEnvelope.event()` builds a FRESH attribute mapping per call and hands it to `core.v1.event.CloudEvent`, because that constructor WRITES its `specversion`, `id`, and `time` defaults into the mapping it is handed and then returns that same live dict from `get_attributes()` — a retained caller mapping is mutated behind its owner's back and a frozen owner holding one is unhashable and unsound. Every default the package injects is already present, so none fires and the mint is total over what the branch already proved.
- Law: `CloudEventValidationError` funnels through ONE `boundary` fence and spreads whole — its `.errors` map is `dict[str, list[BaseCloudEventException]]`, so each attribute's findings become their own `BoundaryFault` and `traversed(by=ACCUMULATE)` reduces them onto the aggregate case. Collapsing to `str(error)` is the deleted form: the aggregating constructor exists precisely so a caller repairs every attribute in one pass rather than one raise at a time.
- Law: the two traces are DISJOINT and both ship. `Extensions.traceparent`/`tracestate`/`baggage` carry the CREATION-time W3C context, injected once at mint; the transport carrier carries the CURRENT hop and is `transport/binding#BINDING`'s. Folding either onto the other loses the leg it alone records — a consumer reading only the carrier sees the hop that delivered the fact and never the operation that produced it.
- Law: the creation-trace extract COMPOSES `execution/admission#CONTEXT` `Correlation.seed` over the extension subset rather than re-spelling `propagate.extract` — the same fold, the same `is_valid` admission over both id bands, the same fall-through to `mint()` on a malformed or absent `traceparent`. One propagator, one adoption law, two carriers.
- Law: attribute digests read the ALPHABETICAL order this owner PUBLISHES, the one order every branch transcribes — `DIGEST_CORE` sorts the core attributes by wire name and `EXTENSION_ROWS` is a key-sorted `Map` over members whose values ARE those names, so the owning values ENFORCE the order rather than asserting it against a declaration sequence a later edit reorders, and `digest_preimage` folds both in that order and length-frames each pair with no call-site sort at all — under `docs/laws/scars.md` `[DIGEST_OVER_UNORDERED_CONTAINER]`, because a mapping's own enumeration derives different bytes in two runtimes and the fixture pinning them never freezes. Any other order is drift the corpus parity proof catches. DSSE material over that digest rides `dssematerial`; the specification's format registry carries no JWS member, so a signature travels as an attribute or not at all.
- Entry: the frozen constructor IS the mint — `MessageEnvelope(event_type, source, operation, occurred, payload, ...)` with every optional slot defaulted — and `MessageEnvelope.decoded(event)` is its inverse over a decoded `BaseCloudEvent`, so both directions of the pair ride one surface. `event()` is the one crossing into the package's own class, so a binding lowers that projection and never holds the owner.
- Law: the pair round-trips EXACTLY. Every slot the message envelope holds is the value that crosses, so `subject` and `dataref` carry the `WireKey` render rather than the typed `ContentKey` a producer minted them from: the estate publishes the content key as bare 32-lowercase-hex, which carries neither `fmt` nor `byte_length`, so a `ContentKey`-typed slot is a value `decoded` cannot rebuild and the inverse silently loses two columns on every decode. `named` is the ONE render, called at the mint where the typed key is still in hand, and the residence carries the producer-side provenance the wire does not.
- Auto: an message envelope carrying `expirytime` past the receiver's own clock is MOOT and settles as a matched-drop half on the receipt rather than a fault, because a stale sample scored against a live one is worse than an unscored one; the drop is evidence, never silence.
- Receipt: this owner mints none — `Announced` carries what the mint proved (the composite, the stamp pair, the ignored inbound extension names, the digest) and the producing surface owns the receipt semantics, exactly as `transport/roots#STORE` splits transport evidence from receipt meaning.
- Growth: a new extension is one `Extension` member, one typed `Extensions` slot whose type the wire render rebuilds, and one `EXTENSION_ROWS` row naming the codec that type already answers — both projections and every consumer stand untouched; a new CARRIED type is one `Codec` beside the four, reaching every slot that holds it; a new required attribute is a specification move, not a branch one; a new severity grade is one vocabulary member.
- Boundary: attribute algebra, its roster, and its digest only. Composes — never re-mints — the `reliability/faults#FAULT` fences, the `execution/admission#CONTEXT` `Correlation` fold and `Classification` vocabulary, and `evidence/identity#IDENTITY`'s key render. Rejected: the whole `cloudevents.v1` tree; a `frozendict[str, str]` extension bag; a caller-retained attribute mapping handed to the constructor; `str(error)` standing in for the aggregating map; a mutable event stored on a frozen owner; a second propagator call site beside the admission fold; a slot whose type the pinned wire spelling cannot rebuild.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import hashlib
from base64 import b64decode, b64encode
from binascii import Error as Base64Error
from collections.abc import Callable
from datetime import datetime
from enum import StrEnum
from typing import Annotated, Any, Final, Self

from expression import Nothing, Ok, Option, Some
from expression.collections import Block, Map
from msgspec import Meta, Raw, Struct

from cloudevents.core.base import BaseCloudEvent
from cloudevents.core.exceptions import CloudEventValidationError
from cloudevents.core.spec import SPECVERSION_V1_0
from cloudevents.core.v1.event import OPTIONAL_ATTRIBUTES, REQUIRED_ATTRIBUTES, CloudEvent

from rasm.runtime.admission import Classification, Correlation
from rasm.runtime.faults import EVENT_EXTENSION, EVENT_MINT, BoundaryFault, Disposition, RuntimeRail, boundary, traversed

# --- [TYPES] ----------------------------------------------------------------------------

type Denominator = Annotated[int, Meta(ge=1)]
type Position = Annotated[int, Meta(ge=0)]
type Attributes = dict[str, Any]

# --- [CONSTANTS] ------------------------------------------------------------------------

# CORE attributes the digest covers, in the canonical order this owner publishes — `REQUIRED_ATTRIBUTES` and
# `OPTIONAL_ATTRIBUTES` are the package's own rosters, so the preimage tracks the specification rather than a
# hand-listed twin, and `sorted` fixes the order a `dict` enumeration would leave to insertion.
DIGEST_CORE: Final[Block[str]] = Block.of_seq(sorted([*REQUIRED_ATTRIBUTES, *OPTIONAL_ATTRIBUTES]))
DIGEST_FRAME: Final[int] = 4  # big-endian length prefix per preimage member; a bare join aliases "ab"+"c" onto "a"+"bc"


class Extension(StrEnum):
    # roster spelled ONCE for the branch. Member VALUES are the wire names, so no row restates them and a name
    # never appears as a literal at a call site. Declaration order is ALPHABETICAL by wire name and that order IS the
    # published canonical digest order every branch transcribes — the `[DIGEST_OVER_UNORDERED_CONTAINER]` scar's
    # required publication, seated on the owning value beside its writer so no reader sorts at its own call site.
    AUTHCONTEXT = "authcontext"
    BAGGAGE = "baggage"
    CORRELATION = "correlation"
    DATACLASSIFICATION = "dataclassification"
    DATAREF = "dataref"
    DEPRECATION = "deprecation"
    DSSEMATERIAL = "dssematerial"
    EXPIRYTIME = "expirytime"
    PARTITIONKEY = "partitionkey"
    RECORDEDTIME = "recordedtime"
    SAMPLEDRATE = "sampledrate"
    SEQUENCE = "sequence"
    SEQUENCETYPE = "sequencetype"
    SEVERITY = "severity"
    TRACEPARENT = "traceparent"
    TRACESTATE = "tracestate"


class Severity(StrEnum):
    # FACT-level operational grade, distinct from the log level its emission renders at: a routine fact logged at
    # warning because the emitter was noisy still routes as routine, and a subscription filters on this alone.
    ROUTINE = "routine"
    NOTABLE = "notable"
    DEGRADED = "degraded"
    CRITICAL = "critical"


class Sequencing(StrEnum):
    # `sequencetype` names the DOMAIN of `sequence`, so a consumer knows whether a gap is a loss or a lawful skip.
    INTEGER = "Integer"


# W3C slots, read as one carrier the admission fold consumes — a subset rather than the whole roster, because
# `propagate.extract` reads the keys it owns and every other extension is this branch's own vocabulary.
TRACE_SLOTS: Final[Block[Extension]] = Block.of_seq([Extension.TRACEPARENT, Extension.TRACESTATE, Extension.BAGGAGE])

# --- [MODELS] ---------------------------------------------------------------------------


class Codec[V](Struct, frozen=True, gc=False):
    # ONE pair per CARRIED type, shared by every slot holding it: `render` lowers the typed value onto the wire and
    # `admit` lifts a wire string back onto the rail, so a slot's codec is chosen by what it holds and no two rows
    # repeat a body between them.
    render: Callable[[V], str]
    admit: Callable[[str], RuntimeRail[V]]


def _vocabulary[V: StrEnum](kind: type[V], /) -> Codec[V]:
    # every closed-vocabulary slot shares one codec: a member renders as its own wire value, and an unknown spelling
    # refuses on the rail rather than reaching a consumer as a string it re-parses.
    return Codec(render=str, admit=lambda raw: boundary(EVENT_EXTENSION, lambda: kind(raw), catch=ValueError))


# ONE rostered anchor serves every extension admit: the refused SPELLING is what a repairing peer needs and the
# caught class carries it, so the per-codec subject the free strings spelled bought a coordinate the detail already
# holds. `b64decode(validate=True)` raises `binascii.Error`, a `ValueError` subclass, so the narrower class is named.
_TEXT: Final[Codec[str]] = Codec(render=str, admit=Ok)
_ORDINAL: Final[Codec[int]] = Codec(render=str, admit=lambda raw: boundary(EVENT_EXTENSION, lambda: int(raw), catch=ValueError))
_INSTANT: Final[Codec[datetime]] = Codec(
    render=datetime.isoformat, admit=lambda raw: boundary(EVENT_EXTENSION, lambda: datetime.fromisoformat(raw), catch=ValueError)
)
_BINARY: Final[Codec[bytes]] = Codec(
    render=lambda held: b64encode(held).decode(),
    admit=lambda raw: boundary(EVENT_EXTENSION, lambda: b64decode(raw, validate=True), catch=Base64Error),
)

# --- [TABLES] -----------------------------------------------------------------------------

# both projections fold THIS table and neither hand-writes an arm. `Map` is key-sorted and `Extension`'s member
# values ARE the wire names, so the table's own iteration IS the published alphabetical digest order — the
# `docs/laws/scars.md` `[DIGEST_OVER_UNORDERED_CONTAINER]` order is enforced by the owning value rather than
# asserted by a declaration sequence a later edit silently reorders.
EXTENSION_ROWS: Final[Map[Extension, Codec[Any]]] = Map.of_seq([
    (Extension.AUTHCONTEXT, _TEXT),
    (Extension.BAGGAGE, _TEXT),
    (Extension.CORRELATION, _TEXT),
    (Extension.DATACLASSIFICATION, _vocabulary(Classification)),
    (Extension.DATAREF, _TEXT),
    (Extension.DEPRECATION, _TEXT),
    (Extension.DSSEMATERIAL, _BINARY),
    (Extension.EXPIRYTIME, _INSTANT),
    (Extension.PARTITIONKEY, _TEXT),
    (Extension.RECORDEDTIME, _INSTANT),
    (Extension.SAMPLEDRATE, _ORDINAL),
    (Extension.SEQUENCE, _ORDINAL),
    (Extension.SEQUENCETYPE, _vocabulary(Sequencing)),
    (Extension.SEVERITY, _vocabulary(Severity)),
    (Extension.TRACEPARENT, _TEXT),
    (Extension.TRACESTATE, _TEXT),
])

# Specification context-attribute types ARE the filter language's own, so the numeric carve DERIVES
# from the codec each slot already answers rather than a second roster `transport/filter#CESQL` keeps parallel
# and a seventeenth extension silently leaves out.
NUMERIC_EXTENSIONS: Final[frozenset[str]] = frozenset(name.value for name, row in EXTENSION_ROWS.items() if row is _ORDINAL)


class Extensions(Struct, frozen=True, gc=False):
    # TYPED slots, never a `frozendict[str, str]` band: each value carries its own domain so a consumer reads a
    # pattern-bounded `WireKey`, a `Classification`, and an aware `datetime` rather than three strings it re-parses, and a
    # spec-invalid name is unspellable rather than admitted silently at a passthrough. Slot order tracks `Extension`'s
    # published alphabetical order, so the struct, the enum, and the digest preimage read one declaration.
    authcontext: Option[str] = Nothing
    baggage: Option[str] = Nothing
    correlation: Option[str] = Nothing
    dataclassification: Option[Classification] = Nothing
    dataref: Option[WireKey] = Nothing
    deprecation: Option[str] = Nothing
    dssematerial: Option[bytes] = Nothing
    expirytime: Option[datetime] = Nothing
    partitionkey: Option[str] = Nothing
    recordedtime: Option[datetime] = Nothing
    sampledrate: Option[Denominator] = Nothing
    sequence: Option[Position] = Nothing
    sequencetype: Option[Sequencing] = Nothing
    severity: Option[Severity] = Nothing
    traceparent: Option[str] = Nothing
    tracestate: Option[str] = Nothing

    def wire(self) -> Map[str, str]:
        # ONE fold over the row table, keyed by the member whose value IS the slot name, so no projection spells a
        # wire name at all; a slot holding `Nothing` contributes no key, because an empty-string value identifies an
        # extension a filter matches and nobody fills.
        return Map.of_seq(
            (name.value, row.render(held)) for name, row in EXTENSION_ROWS.items() for held in getattr(self, name.value).to_list()
        )

    def creation(self) -> Correlation:
        # CREATION-time trace: `Correlation.seed` is admission's own adoption fold — the `is_valid` gate over
        # both id bands, the `is_remote` evidence, the fall-through to `mint()` — composed over the W3C subset of
        # THIS roster. The hop's own carrier is the binding's and never enters here.
        carrier = self.wire()
        return Correlation.seed(Some({slot.value: carrier[slot.value] for slot in TRACE_SLOTS if slot.value in carrier}))


class Announced(Struct, frozen=True, gc=False):
    # what the mint PROVED, never what it did: the composite every dedup reads, the measured lag, the peer names the
    # roster ignored, and the digest a signature covers. Receipt semantics stay the producing surface's.
    composite: Uniqueness
    lag: float
    ignored: Block[str]
    digest: bytes


class MessageEnvelope(Struct, frozen=True, gc=False):
    # canonical owner. `payload` is a `msgspec.Raw` band so the signed sub-tree round-trips byte-identically —
    # a parse-then-reserialize re-spells every float and `-0.0` the signer never saw — and the format lowers those
    # octets rather than a re-encoded projection of them.
    event_type: EventType
    source: Source
    operation: OperationId
    occurred: datetime
    payload: Raw
    subject: Option[WireKey] = Nothing
    data_schema: Option[str] = Nothing
    content_type: Option[str] = Nothing
    extensions: Extensions = Extensions()

    def attributes(self) -> Attributes:
        # a FRESH mapping per call: `CloudEvent.__init__` writes its `specversion`/`id`/`time` defaults INTO the
        # mapping it is handed and `get_attributes()` returns that same live dict, so a retained caller mapping is
        # mutated behind its owner. Every default is already present here, so none of the package's ever fires.
        core: Attributes = {
            "specversion": SPECVERSION_V1_0,
            "id": self.operation.value,
            "source": self.source.reference,
            "type": self.event_type.wire,
            "time": self.occurred,
        }
        optional = {
            "subject": self.subject,
            "dataschema": self.data_schema,
            "datacontenttype": self.content_type,
        }
        return core | {key: held for key, value in optional.items() for held in value.to_list()} | dict(self.extensions.wire())

    def event(self) -> RuntimeRail[CloudEvent]:
        # ONE mint boundary. `CloudEventValidationError.errors` is `dict[attribute, list[exception]]`, so every
        # finding becomes its own fault and `ACCUMULATE` reduces them onto the aggregate — a caller repairs the whole
        # attribute set in one pass, which is exactly what the aggregating constructor exists for.
        return boundary(EVENT_MINT, lambda: CloudEvent(self.attributes(), bytes(self.payload)), catch=CloudEventValidationError).map_error(_spread)

    @classmethod
    def decoded(cls, event: BaseCloudEvent, /) -> RuntimeRail[tuple[Self, Block[str]]]:
        # inverse of `announce` over a decoded event, answering the owner BESIDE the peer extension names the
        # roster ignored — an unknown or over-length name never sheds the fact it rode in on, and the ignored set is
        # decode evidence rather than a silent drop.
        ...

    def digest_preimage(self) -> bytes:
        # canonical order PUBLISHES on the two owning values and nowhere else: `DIGEST_CORE` first, then `EXTENSION_ROWS`
        # in its own key order, both ALPHABETICAL by wire name, each pair length-framed so no two preimages alias. This
        # body sorts nothing — a reader sorting at its own call site desyncs the moment a second reader forgets.
        ...

    def signed(self, sign: Callable[[bytes], bytes], /) -> Self:
        # DSSE over the preimage digest, landing on `dssematerial`. The webhook leg signs the ENCODED bytes once,
        # before any reserialization, so this runs ahead of every format and no relay re-frames what the signer saw.
        ...


def _spread(fault: BoundaryFault, /) -> BoundaryFault:
    # aggregating raises spread WHOLE rather than collapsing to its `__str__`: each attribute's findings land as
    # their own fault under that attribute's subject, so a consumer repairs by name.
    ...
```

## [04]-[FORMAT]

- Owner: `EventFormat` closes the branch's format contract over every `cloudevents.core.formats.base.Format` member, and `FORMAT_ROWS` is the roster every dispatch reads — one row per format carrying its structured media type, its `binary` and `batches` CAPABILITY columns, its codec pair, and the `degrade` each concedes. `JSONFormat` is the package's one shipped implementation and fills the JSON row; the Protobuf and Avro rows are branch-owned implementations of the same protocol, so a format is a ROW and never a type a consumer switches on.
- Cases: `read`/`write` carry the STRUCTURED content mode — the whole message envelope in one body under `application/cloudevents+<suffix>` — while `write_data`/`read_data` carry the BINARY mode, lowering the payload alone while the attributes ride the binding's headers. Every row implements the protocol WHOLE rather than the two members a structured-only consumer needs, because a row silently missing the payload pair breaks every binding's binary leg while its structured leg still runs; a row that cannot serve the pair declares that on its own capability column and refuses by name at both members.
- Law: format capability is SPECIFICATION law read as row data, never a media type derived from a suffix. Avro defines NO batch message envelope and NO binary content mode, so its row carries `binary=False` and `batches=False`, `batch` answers `Nothing` for it, and `encode` over a `Block` under that suffix refuses on the typed rail rather than minting `application/cloudevents-batch+avro`, which no peer implementation resolves. Deriving both media types off `suffix` for every row is the inverted AUTHORITY defect — a branch minting specification surface the specification never defined — and a binding whose row holds `Content.BINARY` still refuses a binary lowering under a format row that does not, because the narrower of the two answers.
- Law: `write_data`/`read_data` ARE the marshaller pair. `cloudevents.v1` threads a `data_marshaller`/`data_unmarshaller` callable through every conversion leg; the `core` tree moved that codec onto the `Format` protocol itself, so the branch's pair is the two protocol members and the legacy parameter family is the refused twin. Each row's pair composes `msgspec` beneath one injected `Frame` codec, so compression is a bound port at the composition root and never a second body path — the same injected-codec law `transport/wire#CRDT_CODEC` holds for its own message envelope.
- Law: framing reads the batch media-type PREFIX, so a batch-carrying format needs no second dispatch — `application/cloudevents-batch+<suffix>` resolves the same row's codec over a sequence, and a new format's batch leg arrives with its row's `batches` column. Prefixes naming a suffix whose row carries no batch message envelope refuse at `framed` rather than decoding one.
- Law: batch framing is BRANCH-owned whole for the two formats that define it. Zero batch code ships — no batch media type, no batch encoder, no batch branch in `JSONFormat` — so `Batch` is this owner's, not a gap routed around. Batches settle PER EVENT and the receipt carries accepted beside matched-duplicate as separate halves; `sequence` survives batching and no re-batch reorders events inside one `source`; a batch past the transport budget splits at the PRODUCER, since a relay re-framing one cannot re-sign it.
- Law: `datacontenttype` is row data off the serdes arrow and never a literal — the format row that encoded the payload names it — so an Avro body under a registry frame never publishes `application/json` and a consumer's decode selection is the producer's own declaration. Absent `datacontenttype` under the JSON format defaults to that format's own `DEFAULT_CONTENT_TYPE`.
- Law: `dataschema` binds the registry subject and its version together, and the `type` major moves WITH that version rather than beside it; a divergent generation refuses at the CONSUMER on every decode, so a producer never negotiates a peer's pinned generation downward.
- Entry: `EventFormat.encode(value)` is one entry over both arities — a `MessageEnvelope` answers a single structured body, a `Block[MessageEnvelope]` answers the batch body under the same row's sibling media type, and a `Block` under a row whose `batches` column is false refuses naming the row — and `EventFormat.decode(body, media)` is its inverse, discriminating on the media type's own batch prefix rather than a caller flag. `get_content_type()` answers the structured type alone, per the protocol.
- Auto: version-factory fallthrough is refused. `core.bindings.common.get_event_factory_for_version` answers `SPECVERSION_V1_0` for EVERY unknown version string, so an unrecognized `specversion` decodes as the current generation rather than refusing; this owner binds `CloudEvent` explicitly and refuses a foreign `specversion` at the seam. `amqp` and `rabbitmq` hard-bind that same class, whose own required-attribute gate REFUSES every `specversion` but the current one — so those two raise where the auto-detecting `http`/`kafka` pair silently decodes a peer's unknown generation as this one.
- Growth: a new format is one `FORMAT_ROWS` row carrying its suffix, its two capability columns, its codec pair, and its `degrade` — the batch leg, the framing dispatch, and every binding inherit it untouched; a new payload frame is one `Frame` port value at the composition root; a new content mode is a specification move, not a branch one.
- Boundary: format contract, its roster, and the batch leg only. Rejected: the `cloudevents.v1` marshaller parameters; a hardwired compression import inside a codec; a second batch dispatch beside the media-type prefix; `get_event_factory_for_version`'s fall-through; a literal `datacontenttype` beside the row that owns it; a batch or binary media type derived off a suffix whose row declares neither capability.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Callable
from enum import StrEnum
from typing import Any, Final, Literal, Protocol

from expression import Nothing, Option, Some
from expression.collections import Block, Map
from msgspec import Raw, Struct

from cloudevents.core.base import BaseCloudEvent, EventFactory
from cloudevents.core.formats.base import Format
from cloudevents.core.formats.json import JSONFormat

from rasm.runtime.faults import RuntimeRail

# --- [TYPES] ----------------------------------------------------------------------------

type Suffix = Literal["json", "protobuf", "avro"]
# payload frames are PORTS, so a codec is composed at the root rather than imported inside a format row — the
# same seam `transport/wire#CRDT_CODEC` opens for its own message envelope, and an unbound frame is the identity pair.
type Frame = tuple[Callable[[bytes], bytes], Callable[[bytes], bytes]]

# --- [CONSTANTS] ------------------------------------------------------------------------

STRUCTURED_STEM: Final[str] = "application/cloudevents+"
BATCH_STEM: Final[str] = "application/cloudevents-batch+"
# ONE refusal spelling both Avro payload members raise, so the bound reads identically at either end of the pair.
_NO_BINARY_MODE: Final[str] = "the avro event format defines no binary content mode; read `FormatRow.binary` first"


class Content(StrEnum):
    # content modes the specification fixes: BINARY splits attributes onto the binding's own headers while the payload
    # rides the body, STRUCTURED puts the whole message envelope in one body, BATCH frames a sequence under the batch stem.
    # `transport/binding#BINDING` rows declare which of the three each protocol holds.
    BINARY = "binary"
    STRUCTURED = "structured"
    BATCH = "batch"

# --- [MODELS] ---------------------------------------------------------------------------


class FormatRow(Struct, frozen=True, gc=False):
    # one row per format: `binary` and `batches` are SPECIFICATION capability, so the batch media type derives off the
    # column rather than off the suffix, `codec` is the `Format` implementation, and `degrade` names what this row
    # forfeits against the coordinates the family carries. Every format defines the structured mode, which is why that
    # one media type derives unconditionally and the other two are columns.
    suffix: Suffix
    codec: Format
    binary: bool
    batches: bool
    fits: str
    admit: str
    lifetime: str
    degrade: tuple[str, ...]

    @property
    def structured(self) -> str:
        return f"{STRUCTURED_STEM}{self.suffix}"

    @property
    def batch(self) -> Option[str]:
        # `Nothing` IS the refusal every consumer reads: a row without a batch message envelope mints no media type, so a
        # caller cannot spell one the peer implementations resolve to nothing.
        return Some(f"{BATCH_STEM}{self.suffix}") if self.batches else Nothing


class ProtobufFormat(Format):
    # branch-owned: the distribution ships no protobuf format at all. `read`/`write` carry the STRUCTURED message envelope
    # through the descriptor generation `dataschema` resolves, while `write_data`/`read_data` carry the payload alone
    # for the binary mode; a payload with no registered descriptor refuses rather than falling back to another row.
    def read(self, event_factory: EventFactory | None, data: str | bytes) -> BaseCloudEvent: ...
    def write(self, event: BaseCloudEvent) -> bytes: ...
    def write_data(self, data: dict[str, Any] | str | bytes | None, datacontenttype: str | None) -> bytes: ...
    def read_data(self, body: bytes, datacontenttype: str | None) -> dict[str, Any] | str | bytes | None: ...
    def get_content_type(self) -> str: ...


class AvroFormat(Format):
    # branch-owned on the same reason, and STRUCTURED-ONLY by specification: the Avro event format defines no batch
    # message envelope and no binary content mode at all, so the payload pair REFUSES by name rather than inventing a lowering
    # no peer decodes. The two members still exist because the protocol declares five and a consumer resolving a row's
    # codec must not meet an attribute error where the row already stated the bound — every caller reads
    # `FormatRow.binary` ahead of them, so the refusal is the floor and never the diagnosis.
    def read(self, event_factory: EventFactory | None, data: str | bytes) -> BaseCloudEvent: ...
    def write(self, event: BaseCloudEvent) -> bytes: ...
    def write_data(self, data: dict[str, Any] | str | bytes | None, datacontenttype: str | None) -> bytes:
        raise NotImplementedError(_NO_BINARY_MODE)

    def read_data(self, body: bytes, datacontenttype: str | None) -> dict[str, Any] | str | bytes | None:
        raise NotImplementedError(_NO_BINARY_MODE)

    def get_content_type(self) -> str: ...


# two branch implementations of that protocol, each composing its own codec beneath one injected
# `Frame`; the distribution ships neither, so their conformance is this branch's own.
_PROTOBUF: Final[Format] = ProtobufFormat()
_AVRO: Final[Format] = AvroFormat()

FORMAT_ROWS: Final[Map[Suffix, FormatRow]] = Map.of_seq(
    (row.suffix, row)
    for row in (
        FormatRow(
            "json",
            codec=JSONFormat(),
            binary=True,
            batches=True,
            fits="the interoperable default every peer implementation reads, and the one format the distribution ships",
            admit="`EventFormat.encode` over the row's own codec; the batch sibling frames a sequence through the same pair",
            lifetime="the consumer's the moment the body is answered; this row holds no buffer across a call",
            degrade=(
                "payload bytes ride base64 under `data_base64`, so a large binary body pays a third more on the wire than the binary content mode does",
                "the package's `write` re-spells every float it parses, so a byte-identical sub-tree crosses as the `Raw` band the digest signs and never as a re-encoded projection",
            ),
        ),
        FormatRow(
            "protobuf",
            codec=_PROTOBUF,
            binary=True,
            batches=True,
            fits="a schema-registered payload crossing to the C# and TypeScript legs under one descriptor generation",
            admit="`EventFormat.encode` over the branch implementation; `dataschema` names the registry subject and version the descriptor resolves",
            lifetime="the consumer's; the descriptor generation outlives the call and refuses on drift rather than transcoding",
            degrade=(
                "the distribution ships no protobuf format, so this row's members are branch-owned and carry no upstream conformance",
                "a payload with no registered descriptor refuses at admission rather than falling back to the JSON row",
            ),
        ),
        FormatRow(
            "avro",
            codec=_AVRO,
            binary=False,
            batches=False,
            fits="a registry-fronted analytic payload whose writer schema evolves independently of every reader",
            admit="`EventFormat.encode` over the branch implementation beside the registry serializer that frames the schema id",
            lifetime="the consumer's; the writer schema resolves per payload off the frame and is never assumed",
            degrade=(
                "the distribution ships no avro format, so this row's members are branch-owned and carry no upstream conformance",
                "the specification defines NO binary content mode here, so every attribute rides inside the body and a consumer routing on an attribute decodes first",
                "the specification defines NO batch message envelope here either, so a sequence crosses as one framed event per message and the batch stem is unspellable",
                "a reader missing the writer schema refuses rather than projecting, since a partial Avro read is a silently wrong record",
            ),
        ),
    )
)


class Batch(Struct, frozen=True, gc=False):
    # branch-owned WHOLE: the distribution carries no batch media type, encoder, or decode branch. Batches settle per
    # event, so the two halves stay separate — a matched duplicate is not an acceptance and folding them erases the
    # exactly-once evidence `Uniqueness` exists to carry.
    accepted: Block[MessageEnvelope]
    duplicate: Block[Uniqueness]


class EventFormat(Protocol):
    # branch format entry, one polymorphic pair over both arities: a `MessageEnvelope` answers the structured body, a
    # `Block[MessageEnvelope]` answers the batch body under the row's own sibling media type, and `decode`
    # discriminates on the media type's own batch PREFIX rather than a caller flag. The plural arm reads `row.batch`
    # and refuses on `Nothing`, so a format the specification gave no batch message envelope is unspellable through this entry
    # rather than encodable into a media type no peer resolves.
    def encode(self, value: MessageEnvelope | Block[MessageEnvelope], /, *, suffix: Suffix) -> RuntimeRail[tuple[str, bytes]]: ...
    def decode(self, body: bytes, /, *, media: str) -> RuntimeRail[MessageEnvelope | Batch]: ...
```

## [05]-[RESEARCH]

(none)

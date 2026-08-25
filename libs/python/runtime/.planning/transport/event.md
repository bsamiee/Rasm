# [PY_RUNTIME_EVENT]


Specification law owns every row and `cloudevents` accelerates it: `core.v1.event.CloudEvent` is the one admitted event class and its aggregating `CloudEventValidationError` funnels through one `boundary` fence into `BoundaryFault`, while the whole `cloudevents.v1` legacy tree — its mutable dict event, its converter stack, its marshaller pair, and its pydantic mirror — is refused by ruling. Where the package's surface is narrower than the specification the branch owns the remainder outright: URI-reference and absolute-URI admission, the extension-name ceiling, the whole batch leg, and every format past JSON. Rails, faults, and the traversal dispositions arrive settled from `reliability/faults#FAULT`; the capability-subject roster from `observability/metrics#METRIC`; the W3C `Correlation` fold from `execution/admission#CONTEXT`; `ContentKey` from `evidence/identity#IDENTITY`; `Hlc` from `evidence/clock#CLOCK`.

## [01]-[INDEX]

- [02]-[GRAMMAR]: attribute grammar — `EventType` segments against the capability-subject roster, `Source`, `OperationId`, the uniqueness composite, and the extension-name ceiling the package does not carry.
- [04]-[FORMAT]: concrete `EventFormat` — instance-bound package codecs, the generated protobuf/Avro publisher assets, JSON/protobuf batch forms, and generated-profile admission.

## [02]-[GRAMMAR]

- Owner: `EventType` is the parsed `rasm.<domain>.<subject>.<fact>` value and the ONE site that spells the pattern, so a producer names a fact by constructing the value rather than formatting a string a subscription later fails to match. `Source` and `OperationId` are its siblings on the required triple, each a refinement over its own domain — `Source` a URI-reference naming the producing CAPABILITY, `OperationId` the producer's operation identity — and `Uniqueness` is the `(source, id)` composite every dedup window and idempotency key reads as one value rather than joining two fields at each consumer.
- Cases: `<domain>` proves against `observability/metrics#METRIC` `DOMAINS` at construction, so a board and a subscription join ONE capability vocabulary and a fact under an unrostered segment refuses exactly as the metric under that segment refuses — the join is the point, and a second event-only segment roster is the fork it forecloses. `<subject>` is the capability's own noun and `<fact>` reads past tense, carrying the announced semantics whole so a semantic break mints a fresh fact spelling; the optional payload-schema URI evolves independently.
- Law: `id` is operation identity and never a content digest — a producer replaying one operation replays one `id`, and the payload's identity rides `subject` under `evidence/identity#IDENTITY`'s `wire` render. `dataref` is instead the URI-reference of an externalized payload residence. Collapsing operation identity, content identity, and residence erases three different joins: deduplication, integrity, and acquisition.
- Law: `source` is the official URI-reference under the Rasm profile's exact `rasm:<domain>/<capability>` spelling. The producer states both segments explicitly; neither derives from `type.subject`, because source context and event type are independent CloudEvents attributes. Profile composition proves only domain agreement between them. The capability therefore names where the occurrence happened without forcing every fact type under that source to repeat the same noun. It is a producer CLAIM verified against the trust row before any routing decision reads it, so it never reaches a filter unverified.
- Law: extension-name ceilings are BRANCH-owned. `core.v1.event.CloudEvent._validate_extension_attributes` matches `^[a-z0-9]+$` over a one-character floor and rejects the reserved `data` name — a charset rule carrying no length bound — so `EXTENSION_FMT` states the specification's twenty-character ceiling and the mint proves it BEFORE construction. Package surfaces narrower than the specification state a fact about the package, never a ceiling on what the estate carries.
- Law: an inbound extension name the roster does not hold, or one past the ceiling, is IGNORED and never a whole-message fault, so a peer's private extension never sheds the fact it rode in on; the ignored set surfaces as decode evidence rather than silence.
- Law: `TYPE_GRAMMAR` is the ONE compiled spelling and both entries cross it — `parse` reads it and `of` renders `_spelled` into it — so a mint proves exactly what a decode proves and neither restates the other's character class. Bare-name sequence arms (`case [TYPE_STEM, domain, ...]`) CAPTURE rather than compare: such an arm rebinds the constant it appears to read and admits every five-segment spelling, so a compiled grammar is what refuses a foreign stem at all and what proves `<subject>` and `<fact>` a producer hands in loose.
- Law: the stamp pair leaves as the MEASURED lag rather than as the two stamps a caller already holds — `stamped` is `Announced.lag`'s one producer, so no line publishes a zero no observation took.
- Law: every refusal resolves ONE `reliability/faults#FAULT` `RAISES` anchor under `RuntimeLeg.EVENT` and derives its subject from that leg, so a fence spells no coordinate its package never declared and the closed `defect` token plus the row's NAMED slots replace the sentences the literal constructions carried. One `EVENT_EXTENSION` anchor serves every extension codec: the refused spelling a peer repairs on rides the caught class the fence names, so the per-codec subject bought nothing the detail did not already hold.
- Entry: `EventType.of(domain, subject, fact)` is the one event-type mint and `EventType.parse(spelling)` the admission it composes, both railed, so the wire spelling round-trips through one owner. `Source.of(domain, capability)` and `Source.parse(reference)` admit the source independently; `_profiled(event_type, source)` is the one Rasm composition gate and proves domain agreement only. `Uniqueness.of(envelope)` projects the composite off a decoded message rather than taking two loose arguments a caller can transpose.
- Growth: a new capability subject is one `DOMAINS` row at the metrics owner, reaching this grammar untouched; a new fact under a standing subject is a `<fact>` value and no declaration at all; a broken event semantic is one fresh `<fact>` spelling, independent of any payload-schema URI move.
- Boundary: attribute grammar only — no transport header spelling, no filter dialect, no subscription. Rejected: a literal `BoundaryFault(...)` construction beside a rostered anchor; a hand-formatted `f"rasm.{...}"` type string beside this owner; a bare-name sequence pattern standing in for a stem comparison; a segment admitted by the mint that the grammar refuses; an event-local capability-segment roster; a `subject` spelling that is not `ContentKey.project("wire")`; a content digest in `id`.

```python
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import re
from datetime import datetime
from typing import Annotated, Final, Self

from expression import Error, Ok
from msgspec import Meta, Struct

from rasm.runtime.faults import EVENT_DOMAIN, EVENT_LAG, EVENT_NAIVE, EVENT_SOURCE, EVENT_TYPE, RuntimeRail
from rasm.runtime.metrics import DOMAINS

# --- [TYPES] ----------------------------------------------------------------------------

type Segment = Annotated[str, Meta(pattern=rf"^{SEGMENT}$")]
type WireKey = Annotated[str, Meta(pattern=r"^[0-9a-f]{32}$")]

# --- [CONSTANTS] ------------------------------------------------------------------------

SEGMENT: Final[str] = r"[a-z0-9]+(?:-[a-z0-9]+)*"
TYPE_STEM: Final[str] = "rasm"
TYPE_GRAMMAR: Final[re.Pattern[str]] = re.compile(
    rf"{TYPE_STEM}\.(?P<domain>{SEGMENT})\.(?P<subject>{SEGMENT})\.(?P<fact>{SEGMENT})"
)
SOURCE_GRAMMAR: Final[re.Pattern[str]] = re.compile(rf"rasm:(?P<domain>{SEGMENT})/(?P<capability>{SEGMENT})")
EXTENSION_FMT: Final[re.Pattern[str]] = re.compile(r"^[a-z0-9]{1,20}$")

# --- [MODELS] ---------------------------------------------------------------------------


class EventType(Struct, frozen=True, order=True, gc=False):
    domain: Segment
    subject: Segment
    fact: Segment

    @classmethod
    def of(cls, domain: str, subject: str, fact: str, /) -> RuntimeRail[Self]:
        return cls.parse(_spelled(domain, subject, fact))

    @classmethod
    def parse(cls, spelling: str, /) -> RuntimeRail[Self]:
        return (
            Error(EVENT_TYPE.raised(spelling))
            if (found := TYPE_GRAMMAR.fullmatch(spelling)) is None
            else Error(EVENT_DOMAIN.raised(found["domain"]))
            if found["domain"] not in DOMAINS
            else Ok(cls(domain=found["domain"], subject=found["subject"], fact=found["fact"]))
        )

    @property
    def wire(self) -> str:
        return _spelled(self.domain, self.subject, self.fact)


class Source(Struct, frozen=True, order=True, gc=False):
    domain: Segment
    capability: Segment

    @classmethod
    def of(cls, domain: str, capability: str, /) -> RuntimeRail[Self]:
        return cls.parse(f"rasm:{domain}/{capability}")

    @classmethod
    def parse(cls, reference: str, /) -> RuntimeRail[Self]:
        return (
            Error(EVENT_SOURCE.raised(reference))
            if (found := SOURCE_GRAMMAR.fullmatch(reference)) is None
            or found["domain"] not in DOMAINS
            else Ok(cls(domain=found["domain"], capability=found["capability"]))
        )

    @property
    def reference(self) -> str:
        return f"rasm:{self.domain}/{self.capability}"


class OperationId(Struct, frozen=True, order=True, gc=False):
    value: Annotated[str, Meta(min_length=1)]


class Uniqueness(Struct, frozen=True, order=True, gc=False):
    source: Source
    operation: OperationId

    @classmethod
    def of(cls, envelope: "MessageEnvelope", /) -> Self:
        return cls(source=envelope.source, operation=envelope.operation)


# --- [OPERATIONS] -----------------------------------------------------------------------


def _spelled(domain: str, subject: str, fact: str, /) -> str:
    return f"{TYPE_STEM}.{domain}.{subject}.{fact}"


def _profiled(event_type: EventType, source: Source, /) -> RuntimeRail[None]:
    return Ok(None) if event_type.domain == source.domain else Error(EVENT_SOURCE.raised(source.reference))


def stamped(produced: datetime, arrived: datetime, /) -> RuntimeRail[float]:
    return (
        Error(EVENT_NAIVE.raised())
        if produced.tzinfo is None or arrived.tzinfo is None
        else Error(EVENT_LAG.raised())
        if arrived < produced
        else Ok((arrived - produced).total_seconds())
    )
```

## [03]-[MESSAGE]

- Cases: each column carries its own domain rather than a generic string bag — `traceparent`/`tracestate`/`baggage`, `partitionkey`, D20 `sequence`, positive `uint32` `sampledrate` capped at the CloudEvents signed-32 integer ceiling, URI-reference `dataref`, producer-creation `recordedtime`, `expirytime`, and string `dataclassification`. Presence is `has_field` — explicit on every optional column — so an unset column contributes no wire key and no `Option` wrapper stands beside the generated slot. Facts outside this interoperable profile stay in typed payload bodies until a real producer/consumer pair earns a profile field. Passthrough `frozendict[str, str]` bands are the deleted form: they mint spec-invalid names at no seam and erase every value type a consumer then re-parses.
- Law: the mint is the ONE boundary. `MessageEnvelope.event()` builds a FRESH attribute mapping per call and hands it to `core.v1.event.CloudEvent`, because that constructor WRITES its `specversion`, `id`, and `time` defaults into the mapping it is handed and then returns that same live dict from `get_attributes()` — a retained caller mapping is mutated behind its owner's back and a frozen owner holding one is unhashable and unsound. Every default the package injects is already present, so none fires and the mint is total over what the branch already proved.
- Law: `CloudEventValidationError` funnels through ONE mint fence and spreads whole — its `.errors` map is `dict[str, list[BaseCloudEventException]]`, so each attribute/finding-class pair becomes its own `BoundaryFault` and `combine` reduces them onto the aggregate case. The package exposes no stable finding code or tag, making the exception class the only non-message discriminant. Collapsing to `str(error)` is the deleted form: the aggregating constructor exists precisely so a caller repairs every attribute in one pass rather than one raise at a time.
- Law: the two traces are DISJOINT and both ship. `traceparent`/`tracestate`/`baggage` carry the CREATION-time W3C context, injected once at mint; the transport carrier carries the CURRENT hop and is `transport/binding#BINDING`'s. `TRACE_SLOTS` is the intersection of the generated roster with the global propagator's own `fields`, so the W3C subset derives from the propagator rather than a hand list. Folding either trace onto the other loses the leg it alone records.
- Law: the creation-trace extract COMPOSES `execution/admission#CONTEXT` `Correlation.seed` over that subset rather than re-spelling `propagate.extract` — the same fold, the same `is_valid` admission over both id bands, the same fall-through to `mint()` on a malformed or absent `traceparent`. One propagator, one adoption law, two carriers.
- Entry: the frozen constructor IS the mint — `MessageEnvelope(event_type, source, operation, occurred, payload, ...)` with every optional slot defaulted — and `MessageEnvelope.decoded(event)` is its inverse over a decoded `BaseCloudEvent`, so both directions of the pair ride one surface; `rendered(extensions)` and `admitted(attributes)` are the two projections of the generated roster both directions compose. `event()` is the one crossing into the package's own class, so a binding lowers that projection and never holds the owner.
- Law: the pair round-trips EXACTLY. Opaque data enters as `Raw` and leaves as the same bytes; generated protobuf data enters and leaves as the generated `Message` itself, including a decoded `Any` with its `type_url` intact; reference-only data remains absent while `dataref` locates the identical bytes. No producer-side pack, cast, wrapper, or parallel envelope intervenes. `subject` carries the `WireKey` render rather than the typed `ContentKey` a producer minted it from: the estate publishes that content key as bare 32-lowercase-hex, which carries neither `fmt` nor `byte_length`, so a `ContentKey`-typed slot would fabricate two columns on decode. `dataref` is separately a validated URI-reference naming the external residence and never masquerades as that content key.
- Auto: a message envelope carrying `expirytime` past the receiver's own clock is MOOT and settles as a matched-drop half on the settlement rather than a fault, because a stale sample scored against a live one is worse than an unscored one; the drop is evidence, never silence.
- Output: `Announced` carries what the mint proved — the composite, the measured arrival lag, and the ignored inbound extension names — and the producing surface owns the outcome semantics, exactly as `transport/roots#STORE` splits transport evidence from outcome meaning.
- Boundary: attribute algebra and its generated roster only. Composes — never re-mints — the `reliability/faults#FAULT` fences, the `execution/admission#CONTEXT` `Correlation` fold, and `evidence/identity#IDENTITY`'s key render. Rejected: the whole `cloudevents.v1` tree; a `frozendict[str, str]` extension bag; a msgspec struct restating the generated `Extensions`; a local payload wrapper around generated `Message`; a hand extension-name roster beside the descriptor; a caller-retained attribute mapping handed to the constructor; `str(error)` standing in for the aggregating map; a second propagator call site beside the admission fold; a slot whose type the pinned wire spelling cannot rebuild.

```python
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Callable, Mapping
from datetime import datetime
from typing import Any, Final, Literal, Self, cast
from urllib.parse import urlsplit

from expression import Error, Nothing, Ok, Option, Some, effect
from expression.collections import Block, Map
from msgspec import Raw, Struct, ValidationError as ShapeValidationError, convert, field
from opentelemetry import propagate
from protobuf import DescField, DescFieldValueMessage, DescFieldValueScalar, Message, Oneof, ScalarType
from protobuf.wkt import Timestamp
from protovalidate import CompilationError, EvaluationError, ValidationError as ContractValidationError, validate
# Contracts are retired from this logic.

from cloudevents.core.base import BaseCloudEvent
from cloudevents.core.exceptions import CloudEventValidationError
from cloudevents.core.spec import SPECVERSION_V1_0
from cloudevents.core.v1.event import OPTIONAL_ATTRIBUTES, REQUIRED_ATTRIBUTES, CloudEvent

from rasm.runtime.admission import Correlation
from rasm.runtime.faults import EVENT_EXTENSION, EVENT_MINT, BoundaryFault, Disposition, RuntimeRail, boundary, traversed

# --- [TYPES] ----------------------------------------------------------------------------

type Attributes = dict[str, Any]
type AttributeArm = Literal["ce_integer", "ce_string", "ce_uri_ref", "ce_timestamp"]
type EventData = Raw | Message | None

# --- [CONSTANTS] ------------------------------------------------------------------------

CORE_ATTRIBUTES: Final[frozenset[str]] = frozenset(REQUIRED_ATTRIBUTES) | frozenset(OPTIONAL_ATTRIBUTES)
URI_REFERENCE_FMT: Final[re.Pattern[str]] = re.compile(
    r"^(?:[A-Za-z0-9._~:/?#\[\]@!$&'()*+,;=-]|%[0-9A-Fa-f]{2})*$"
)


# --- [MODELS] ---------------------------------------------------------------------------


class Codec[V](Struct, frozen=True, gc=False):
    arm: AttributeArm
    render: Callable[[V], object]
    admit: Callable[[object], RuntimeRail[V]]


class CloudEventUri(str):
    __slots__ = ()


class CloudEventUriRef(str):
    __slots__ = ()


_TEXT: Final[Codec[str]] = Codec(
    arm="ce_string",
    render=lambda held: held,
    admit=lambda raw: Ok(raw) if isinstance(raw, str) else Error(EVENT_EXTENSION.raised()),
)
_URI_REF: Final[Codec[str]] = Codec(
    arm="ce_uri_ref",
    render=lambda held: held,
    admit=lambda raw: Ok(raw) if isinstance(raw, str) else Error(EVENT_EXTENSION.raised()),
)
_ORDINAL: Final[Codec[int]] = Codec(
    arm="ce_integer",
    render=lambda held: held,
    admit=lambda raw: _ordinal(raw),
)
_INSTANT: Final[Codec[Timestamp]] = Codec(
    arm="ce_timestamp",
    render=lambda held: held.to_datetime(),
    admit=lambda raw: _instant(raw),
)


def _ordinal(raw: object, /) -> RuntimeRail[int]:
    parsed = (
        Error(EVENT_EXTENSION.raised())
        if isinstance(raw, bool)
        else Ok(raw)
        if isinstance(raw, int)
        else boundary(EVENT_EXTENSION, lambda: int(raw), catch=ValueError)
        if isinstance(raw, str)
        else Error(EVENT_EXTENSION.raised())
    )
    return parsed.bind(
        lambda value: boundary(EVENT_EXTENSION, lambda: _ce_integer(value), catch=OverflowError)
    )


def _ce_integer(value: int, /) -> int:
    if -(1 << 31) <= value < (1 << 31):
        return value
    raise OverflowError("CloudEvents integer exceeds the signed 32-bit abstract-type domain")


def _instant(raw: object, /) -> RuntimeRail[Timestamp]:
    if isinstance(raw, datetime):
        return _timestamp(raw)
    if isinstance(raw, str):
        return boundary(EVENT_EXTENSION, lambda: datetime.fromisoformat(raw), catch=ValueError).bind(_timestamp)
    return Error(EVENT_EXTENSION.raised())


def _timestamp(raw: datetime, /) -> RuntimeRail[Timestamp]:
    return Error(EVENT_EXTENSION.raised()) if raw.tzinfo is None else Ok(Timestamp.from_datetime(raw))


def _uri_reference(column: DescField, /) -> bool:
    options = column.proto.options
    if options is None or ext_field not in options:
        return False
    match options[ext_field].type:
        case Oneof(field="string", value=rules):
            return rules.well_known == Oneof("uri_ref", True)
        case _:
            return False


def _codec(column: DescField) -> Codec[Any]:
    if EXTENSION_FMT.fullmatch(column.local_name) is None:
        raise TypeError(column.local_name)
    match column.value:
        case DescFieldValueScalar(scalar=ScalarType.STRING):
            return _URI_REF if _uri_reference(column) else _TEXT
        case DescFieldValueScalar(scalar=ScalarType.UINT32 | ScalarType.UINT64):
            return _ORDINAL
        case DescFieldValueMessage(message=message) if message.type_name == "google.protobuf.Timestamp":
            return _INSTANT
        case unrowed:
            raise TypeError(f"{column.local_name}:{type(unrowed).__name__}")


# --- [TABLES] ---------------------------------------------------------------------------

EXTENSION_ROWS: Final[Map[str, Codec[Any]]] = Map.of_seq(
    (column.local_name, _codec(column)) for column in Extensions.desc().fields
)

NUMERIC_EXTENSIONS: Final[frozenset[str]] = frozenset(name for name, row in EXTENSION_ROWS.items() if row is _ORDINAL)

TRACE_SLOTS: Final[Block[str]] = Block.of_seq(sorted(frozenset(EXTENSION_ROWS.keys()) & frozenset(propagate.get_global_textmap().fields)))


class Announced(Struct, frozen=True, gc=False):
    composite: Uniqueness
    lag: float
    ignored: Block[str]


class MessageEnvelope(Struct, frozen=True, gc=False):
    event_type: EventType
    source: Source
    operation: OperationId
    occurred: datetime
    payload: EventData
    subject: Option[WireKey] = Nothing
    data_schema: Option[str] = Nothing
    content_type: Option[str] = Nothing
    extensions: Extensions = field(default_factory=Extensions)

    def attributes(self) -> Attributes:
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
        return core | {key: held for key, value in optional.items() for held in value.to_list()} | dict(rendered(self.extensions))

    def event(self) -> RuntimeRail[CloudEvent]:
        return _profiled(self.event_type, self.source).bind(lambda _: _schema(self.data_schema)).bind(
            lambda _: boundary(
                EVENT_EXTENSION,
                lambda: _validated(self.extensions),
                catch=(TypeError, ValueError, OverflowError, ContractValidationError, CompilationError, EvaluationError),
            )
        ).bind(_renderable).bind(lambda _: _minted(self.attributes(), self.payload))

    @classmethod
    @effect.result[tuple[Self, Block[str]], BoundaryFault]()
    def decoded(cls, event: BaseCloudEvent, /) -> RuntimeRail[tuple[Self, Block[str]]]:
        attributes = event.get_attributes()
        extensions, ignored = admitted(attributes)
        event_type = yield from EventType.parse(event.get_type())
        source = yield from Source.parse(event.get_source())
        yield from _profiled(event_type, source)
        occurred, payload = yield from _body(event)
        subject = yield from _subject(event.get_subject())
        return (
            cls(
                event_type=event_type,
                source=source,
                operation=OperationId(value=event.get_id()),
                occurred=occurred,
                payload=payload,
                subject=subject,
                data_schema=(yield from _schema(Option.of_optional(event.get_dataschema()))),
                content_type=Option.of_optional(event.get_datacontenttype()),
                extensions=(yield from extensions),
            ),
            ignored,
        )


# --- [OPERATIONS] -----------------------------------------------------------------------


def rendered(extensions: Extensions) -> Map[str, object]:
    return Map.of_seq((name, row.render(getattr(extensions, name))) for name, row in EXTENSION_ROWS.items() if extensions.has_field(name))


def admitted(attributes: Mapping[str, object]) -> tuple[RuntimeRail[Extensions], Block[str]]:
    rostered = Block.of_seq((name, _admitted(row, attributes[name])) for name, row in EXTENSION_ROWS.items() if name in attributes)
    ignored = Block.of_seq(sorted(name for name in attributes if name not in EXTENSION_ROWS and name not in CORE_ATTRIBUTES))
    lifted = traversed(rostered.map(lambda cell: cell[1].map(lambda value: (cell[0], value))), by=Disposition.ACCUMULATE)
    return lifted.bind(
        lambda cells: boundary(
            EVENT_EXTENSION,
            lambda: _validated(Extensions(**dict(cells))),
            catch=(TypeError, ValueError, OverflowError, ContractValidationError, CompilationError, EvaluationError),
        )
    ), ignored


def _validated(extensions: Extensions, /) -> Extensions:
    extensions.to_binary()
    validate(extensions)
    return extensions


def _renderable(extensions: Extensions, /) -> RuntimeRail[Extensions]:
    cells = Block.of_seq(
        boundary(
            EVENT_EXTENSION,
            lambda row=row, name=name: row.render(getattr(extensions, name)),
            catch=(TypeError, ValueError, OverflowError),
        ).bind(row.admit)
        for name, row in EXTENSION_ROWS.items()
        if extensions.has_field(name)
    )
    return traversed(cells, by=Disposition.ACCUMULATE).map(lambda _: extensions)


def _admitted(row: Codec[Any], raw: object, /) -> RuntimeRail[Any]:
    return row.admit(raw)


def _body(event: BaseCloudEvent, /) -> RuntimeRail[tuple[datetime, EventData]]:
    occurred, payload = event.get_time(), event.get_data()
    return (
        Error(EVENT_MINT.raised("time", "InvalidAttributeTypeError"))
        if not isinstance(occurred, datetime)
        else Error(EVENT_MINT.raised("time", "InvalidAttributeValueError"))
        if occurred.tzinfo is None
        else Ok((occurred, Raw(payload)))
        if isinstance(payload, bytes)
        else Ok((occurred, payload))
        if isinstance(payload, Message)
        else Ok((occurred, None))
        if payload is None
        else Error(EVENT_MINT.raised("data", "InvalidAttributeTypeError"))
    )


def _subject(raw: str | None, /) -> RuntimeRail[Option[WireKey]]:
    if raw is None:
        return Ok(Nothing)
    try:
        return Ok(Some(convert(raw, WireKey)))
    except ShapeValidationError:
        return Error(EVENT_MINT.raised("subject", "ValidationError"))


def _schema(value: Option[str], /) -> RuntimeRail[Option[str]]:
    return value.map(lambda raw: _absolute(raw).map(Some)).default_value(Ok(Nothing))


def _uri_reference(raw: object, name: str, /, *, nonempty: bool = True) -> RuntimeRail[str]:
    if (
        not isinstance(raw, str)
        or nonempty and not raw
        or URI_REFERENCE_FMT.fullmatch(raw) is None
    ):
        return Error(EVENT_MINT.raised(name, "InvalidAttributeValueError"))
    try:
        parsed = urlsplit(raw)
        _ = parsed.port
    except ValueError:
        return Error(EVENT_MINT.raised(name, "InvalidAttributeValueError"))
    first = parsed.path.partition("/")[0]
    return (
        Error(EVENT_MINT.raised(name, "InvalidAttributeValueError"))
        if not parsed.scheme and not parsed.netloc and ":" in first
        else Ok(raw)
    )


def _absolute(raw: str, name: str = "dataschema", /) -> RuntimeRail[str]:
    return _uri_reference(raw, name).bind(
        lambda admitted: Ok(admitted)
        if urlsplit(admitted).scheme
        else Error(EVENT_MINT.raised(name, "InvalidAttributeValueError"))
    )


def _event(attributes: dict[str, Any], data: object, /) -> CloudEvent:
    factory = cast(Callable[[dict[str, Any], object], CloudEvent], CloudEvent)
    return factory(attributes, data)


def _minted(attributes: Attributes, payload: object, /) -> RuntimeRail[CloudEvent]:
    try:
        return Ok(_event(attributes, bytes(payload) if isinstance(payload, Raw) else payload))
    except CloudEventValidationError as refused:
        faults = Block.of_seq(
            EVENT_MINT.raised(attribute, type(finding).__name__)
            for attribute, findings in refused.errors.items()
            for finding in findings
        )
        return Error(faults.reduce(BoundaryFault.combine))


def _strict(event: BaseCloudEvent, /) -> RuntimeRail[CloudEvent]:
    attributes = dict(event.get_attributes())
    source = _uri_reference(attributes.get("source"), "source")
    schema = (
        Ok(None)
        if (raw_schema := attributes.get("dataschema")) is None
        else _absolute(raw_schema).map(lambda _: None)
        if isinstance(raw_schema, str)
        else Error(EVENT_MINT.raised("dataschema", "InvalidAttributeTypeError"))
    )
    typed = tuple(
        (
            _absolute(str(value), name)
            if isinstance(value, CloudEventUri)
            else _uri_reference(str(value), name, nonempty=False)
        ).map(
            lambda _: None
        )
        for name, value in attributes.items()
        if isinstance(value, (CloudEventUri, CloudEventUriRef)) and name != "dataschema"
    )
    return traversed(
        Block.of_seq((source.map(lambda _: None), schema, *typed)),
        by=Disposition.ACCUMULATE,
    ).bind(
        lambda _: _minted(attributes, event.get_data())
    )


def creation(extensions: Extensions) -> Correlation:
    carrier = rendered(extensions)
    trace: dict[str, str] = {}
    for slot in TRACE_SLOTS:
        if slot in carrier and isinstance(value := carrier[slot], str):
            trace[slot] = value
    return Correlation.seed(Some(trace))


```

## [04]-[FORMAT]

- Cases: `read`/`write` carry the STRUCTURED content mode — the whole message envelope in one body under `application/cloudevents+<suffix>` — while `write_data`/`read_data` carry the BINARY mode, lowering the payload alone while the attributes ride the binding's headers. Every row implements the protocol WHOLE rather than the two members a structured-only consumer needs, because a row silently missing the payload pair breaks every binding's binary leg while its structured leg still runs; a row that cannot serve the pair declares that on its own capability column and refuses by name at both members.
- Law: on the protobuf row the envelope's `data` oneof IS the content-mode election: `proto_data` carries a generated message packed as `Any`, whose own `type_url` resolves through the separately configured descriptor registry; `binary_data` carries opaque octets; `text_data` a textual body. `dataschema`, when present, remains the independent absolute URI of the data schema and is never synthesized from `Any.type_url`. The four required attributes ride the envelope's own fields and every other attribute the `attributes` map as a `CloudEventAttributeValue` whose oneof arm is the attribute's TYPE — `ce_timestamp` for `time` and the `Timestamp` extensions, `ce_integer` for `sampledrate`, `ce_uri` for `dataschema`, `ce_uri_ref` for `dataref`, and `ce_string` for D20 `sequence` and remaining text. Decode proves the elected `attr.field` equals the exact standard/profile arm BEFORE extracting `attr.value`; protobuf default reads from an inactive scalar arm never become admitted values. A `specversion` other than 1.0 refuses at the seam: this row binds `CloudEvent` explicitly and never the version-detecting factory fall-through.
- Law: format capability is specification law read as row data. Every format defines singular structured mode; only JSON and protobuf publish batch forms, so only those rows bind a `BatchCodec`. Binary is a protocol-binding content mode, not an event-format suffix: opaque bytes cross unchanged and a generated `Message` uses its generated binary encoder while attributes ride the carrier. Callers cannot ask for "binary Avro" or "binary JSON" and no `binary` capability column lies about it.
- Law: structured payload capability is row data, not a provider exception discovered after selection. JSON and Avro carry the profile's opaque bytes; protobuf also carries a generated `Message` through `Any`. A request whose payload arm the selected row cannot represent refuses before mint or codec invocation, and neither JSON nor Avro invents a protobuf-to-JSON or wrapper projection.
- Law: the generic Avro codec realizes the exact publisher union whole — bytes/null/boolean/finite number/string, recursive JSON objects, and the schema's record-object arrays. Its `AvroCloudEventData` wrappers exist only on the Avro wire and disappear on read; arbitrary host objects and generated protobuf messages refuse. Explicit Rasm-profile admission remains the later opaque-bytes/generated-Message gate, so generic format capability is never erased to match one application profile.
- Law: every generic entry re-mints through the strict v1 owner before bytes leave or decoded values return. The branch closes the SDK's URI gap by proving `source` as a non-empty URI-reference and `dataschema` as an absolute URI. Protobuf's explicit `ce_uri` and `ce_uri_ref` extension arms decode into `CloudEventUri`/`CloudEventUriRef`; admission proves each marker's own URI domain and a later protobuf write retains the elected abstract type without a parallel envelope or attribute roster.
- Law: `write_data`/`read_data` complete the upstream `Format` protocol but do not mint a second payload rail. The sealed payload union lowers directly; a binary read remains opaque because no `Any.type_url` rides that content mode. Compression belongs after the complete transport body at the binding/residence owner; the deleted `Frame` identity pair had no binding site, failure declaration, or consumer.
- Law: framing parses the complete MIME value through the standard-library header registry before dispatch. A malformed parameter tail, duplicate/broken parameter, newline, or invalid token refuses even when the leading stem looks valid; binding raise reuses this exact parser and never strips at the first semicolon. `application/cloudevents-batch+<suffix>` then resolves the same row's optional `batch_codec`. On protobuf the body is the generated `CloudEventBatch`; on JSON it is the specification array of complete JSON-format event objects. An absent codec is the typed refusal, so Avro never mints a batch media type.
- Law: decode returns package `BaseCloudEvent` values, retaining every standard payload arm; explicit `admit` produces `Decoded(events, ignored)` only after the sealed opaque-bytes/generated-Message profile proves each event. A protobuf `proto_data` arm remains its generated `Any` with `type_url` intact. Duplicate detection, acceptance, and settlement require journal and binding state a decoder does not possess; batch position carries no event order.
- Law: `datacontenttype` describes event DATA and never the enclosing event format. The structured message's media type is `Encoded.media`; `dataschema` remains the optional absolute URI for the data schema; Avro's frozen event schema is neither coordinate. `Any.type_url`, payload schema URI, event type, and contract generation remain independent.
- Law: `dataschema` is an optional absolute URI identifying the schema `data` adheres to; the event type, the registry subject and version, the protobuf package, and `Any.type_url` remain independent concepts. Divergent contract generations refuse at the CONSUMER through separately configured contract and registry state, so a producer never negotiates a peer's pinned generation downward.
- Entry: `EventFormat.bound()` resolves and parses the generated AVSC once; `AvroFormat.read` is the exact publisher-Avro byte reader that `decode(body, media)` dispatches. Generic `write(event, suffix)` and `decode(body, media)` cover package CloudEvent singles and batches without narrowing their payload arm. Profile `encode(envelope, suffix)` first admits the row's sealed payload arm and mints the package event; `admit(events)` is the inverse Rasm-profile gate, and `admitted(event)` gives binary bindings the same gate.
- Auto: version-factory fallthrough is refused. `core.bindings.common.get_event_factory_for_version` answers `SPECVERSION_V1_0` for EVERY unknown version string, so an unrecognized `specversion` decodes as the current generation rather than refusing; every row here binds `CloudEvent` explicitly and refuses a foreign `specversion` at the seam. `amqp` and `rabbitmq` hard-bind that same class, whose own required-attribute gate REFUSES every `specversion` but the current one — so those two raise where the auto-detecting `http`/`kafka` pair silently decodes a peer's unknown generation as this one.
- Boundary: event-format serialization and profile admission only. Rejected: a registry frame around the CloudEvents Avro envelope; a hand AVSC constant; a tests/ asset read at runtime; an identity compression port; version-factory fallthrough; settlement inside decode; a batch media type with no publisher format.

```python
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Callable
from dataclasses import dataclass
from datetime import datetime
from email import policy
from email.parser import HeaderParser
from enum import StrEnum
from importlib.resources import files
from io import BytesIO
from json import JSONDecodeError
from math import isfinite
from typing import Any, Final, Literal, assert_never, cast

import msgspec
from dateutil.parser import isoparse
from expression import Error, Nothing, Ok, Option, Some
from expression.collections import Block, Map
from expression.extra.result import traverse
from fastavro import parse_schema, schemaless_reader, schemaless_writer
from fastavro.read import SchemaResolutionError
from fastavro.schema import SchemaParseException, UnknownType
from fastavro.types import DictSchema
from msgspec import Raw, Struct
from protobuf import Message, Oneof, wkt
# Contracts are retired from this logic.

from cloudevents.core.base import BaseCloudEvent, EventFactory
from cloudevents.core.exceptions import CloudEventValidationError
from cloudevents.core.formats.base import Format
from cloudevents.core.formats.json import JSONFormat
from cloudevents.core.spec import SPECVERSION_V1_0
from cloudevents.core.v1.event import CloudEvent

from rasm.runtime.faults import EVENT_DECODE, EVENT_ENCODE, EVENT_FORMAT, Catch, RuntimeRail, boundary

# --- [TYPES] ----------------------------------------------------------------------------

type Suffix = Literal["json", "protobuf", "avro"]
type Payload = Message | dict[str, Any] | str | bytes | None

# --- [CONSTANTS] ------------------------------------------------------------------------

STRUCTURED_STEM: Final[str] = "application/cloudevents+"
BATCH_STEM: Final[str] = "application/cloudevents-batch+"
_ENVELOPE_FIELDS: Final[frozenset[str]] = frozenset({"id", "source", "specversion", "type"})
_AVRO_RESOURCE: Final[str] = "vendor/io/cloudevents/v1/cloudevents.avsc"
_FORMAT_RAISES: Final[Catch] = (
    CloudEventValidationError,
    EOFError,
    JSONDecodeError,
    msgspec.DecodeError,
    RecursionError,
    SchemaParseException,
    SchemaResolutionError,
    TypeError,
    UnicodeDecodeError,
    UnknownType,
    ValueError,
    OverflowError,
)


class Content(StrEnum):
    BINARY = "binary"
    STRUCTURED = "structured"
    BATCH = "batch"


class PayloadArm(StrEnum):
    OPAQUE = "opaque"
    MESSAGE = "message"

# --- [MODELS] ---------------------------------------------------------------------------


class FormatRow(Struct, frozen=True, gc=False):
    suffix: Suffix
    codec: Format
    batch_codec: Option["BatchCodec"]
    payloads: frozenset[PayloadArm]
    fits: str
    admit: str
    lifetime: str
    degrade: tuple[str, ...]

    @property
    def structured(self) -> str:
        return f"{STRUCTURED_STEM}{self.suffix}"

    @property
    def batch(self) -> Option[str]:
        return self.batch_codec.map(lambda _: f"{BATCH_STEM}{self.suffix}")


class MediaType(Struct, frozen=True, gc=False):
    maintype: str
    subtype: str
    parameters: tuple[tuple[str, str], ...]


class BatchCodec(Struct, frozen=True, gc=False):
    write: Callable[[Block[BaseCloudEvent]], bytes]
    read: Callable[[bytes], Block[BaseCloudEvent]]
    raises: Catch


class ProtobufFormat(Format):
    def read(self, event_factory: EventFactory | None, data: str | bytes) -> BaseCloudEvent:
        wire = cloudevents_pb.CloudEvent.from_binary(data if isinstance(data, bytes) else data.encode())
        if event_factory is not None and event_factory is not CloudEvent:
            raise ValueError("protobuf format binds core.v1.event.CloudEvent")
        return _event(_attributes(wire), _payload(wire))

    def write(self, event: BaseCloudEvent) -> bytes:
        return _wired(event).to_binary()

    def write_data(self, data: Payload, datacontenttype: str | None) -> bytes:
        return _payload_bytes(data)

    def read_data(self, body: bytes, datacontenttype: str | None) -> Payload:
        return body

    def get_content_type(self) -> str:
        return f"{STRUCTURED_STEM}protobuf"

    @staticmethod
    def batch(events: Block[BaseCloudEvent]) -> bytes:
        return cloudevents_pb.CloudEventBatch(events=[_wired(event) for event in events]).to_binary()

    @staticmethod
    def unbatch(body: bytes) -> Block[BaseCloudEvent]:
        return Block.of_seq(
            _event(_attributes(wire), _payload(wire))
            for wire in cloudevents_pb.CloudEventBatch.from_binary(body).events
        )


@dataclass(frozen=True, slots=True)
class AvroFormat(Format):
    _schema: DictSchema

    def read(self, event_factory: EventFactory | None, data: str | bytes) -> BaseCloudEvent:
        if event_factory is not None and event_factory is not CloudEvent:
            raise ValueError("avro format binds core.v1.event.CloudEvent")
        body = data if isinstance(data, bytes) else data.encode()
        record = schemaless_reader(BytesIO(body), self._schema)
        attributes = {name: value for name, value in record["attribute"].items() if value is not None}
        if attributes.get("specversion") != SPECVERSION_V1_0:
            raise ValueError(str(attributes.get("specversion")))
        if isinstance(stamp := attributes.get("time"), str):
            attributes["time"] = isoparse(stamp)
        return _event(attributes, _avro_data_decode(record["data"]))

    def write(self, event: BaseCloudEvent) -> bytes:
        target = BytesIO()
        attributes = {
            name: _avro_attribute(value)
            for name, value in event.get_attributes().items()
            if value is not None
        }
        schemaless_writer(
            target,
            self._schema,
            {"attribute": attributes, "data": _avro_data_encode(event.get_data())},
            strict=True,
        )
        return target.getvalue()

    def write_data(self, data: Payload, datacontenttype: str | None) -> bytes:
        return _payload_bytes(data)

    def read_data(self, body: bytes, datacontenttype: str | None) -> Payload:
        return body

    def get_content_type(self) -> str:
        return f"{STRUCTURED_STEM}avro"


class Encoded(Struct, frozen=True, gc=False):
    media: str
    body: bytes


class Decoded(Struct, frozen=True, gc=False):
    events: Block[MessageEnvelope]
    ignored: Block[str]


class EventFormat(Struct, frozen=True, gc=False):
    rows: Map[Suffix, FormatRow]

    @classmethod
    def bound(cls) -> RuntimeRail["EventFormat"]:
        resource = files("rasm.contracts").joinpath(_AVRO_RESOURCE)
        return boundary(
            EVENT_FORMAT,
            lambda: cast(DictSchema, parse_schema(msgspec.json.decode(resource.read_bytes()))),
            catch=(
                FileNotFoundError,
                ModuleNotFoundError,
                OSError,
                SchemaParseException,
                UnknownType,
                msgspec.DecodeError,
            ),
        ).map(lambda schema: cls(rows=_format_rows(schema)))

    def encode(
        self, value: MessageEnvelope | Block[MessageEnvelope], /, *, suffix: Suffix
    ) -> RuntimeRail[Encoded]:
        return self._row(suffix, "structured").bind(
            lambda row: self._payloads(row, value).bind(lambda _: self._encoded(row, value))
        )

    def write(
        self, value: BaseCloudEvent | Block[BaseCloudEvent], /, *, suffix: Suffix
    ) -> RuntimeRail[Encoded]:
        return self._row(suffix, "structured").bind(
            lambda row: _strict_events(value).bind(lambda admitted: self._written(row, admitted))
        )

    def decode(self, body: bytes, /, *, media: str) -> RuntimeRail[Block[BaseCloudEvent]]:
        return self._framed(media).bind(lambda framed: self._decoded(framed[0], framed[1], body)).bind(
            lambda events: traverse(_strict, events).map(
                lambda admitted: cast(Block[BaseCloudEvent], admitted)
            )
        )

    def admit(self, events: Block[BaseCloudEvent], /) -> RuntimeRail[Decoded]:
        return _admitted_events(events)

    def admitted(self, event: BaseCloudEvent, /) -> RuntimeRail[Decoded]:
        return self.admit(Block.singleton(event))

    def codec(self, suffix: Suffix, envelope: MessageEnvelope, /) -> RuntimeRail[Format]:
        return self._row(suffix, "structured").bind(
            lambda row: self._payloads(row, envelope).map(lambda _: row.codec)
        )

    @property
    def payload_codec(self) -> Format:
        return self.rows["protobuf"].codec

    def _decoded(self, row: FormatRow, batch: bool, body: bytes, /) -> RuntimeRail[Block[BaseCloudEvent]]:
        if batch:
            return row.batch_codec.to_result_with(lambda: EVENT_FORMAT.raised(row.suffix, "batch")).bind(
                lambda codec: boundary(EVENT_DECODE, lambda: codec.read(body), catch=codec.raises)
            )
        return boundary(
            EVENT_DECODE,
            lambda: Block.singleton(row.codec.read(CloudEvent, body)),
            catch=_FORMAT_RAISES,
        )

    def _encoded(
        self, row: FormatRow, value: MessageEnvelope | Block[MessageEnvelope], /
    ) -> RuntimeRail[Encoded]:
        match value:
            case MessageEnvelope() as envelope:
                return envelope.event().bind(lambda event: self._written(row, event))
            case Block() as envelopes:
                return traverse(lambda envelope: envelope.event(), envelopes).bind(
                    lambda events: self._written(row, events)
                )
            case _ as unreachable:
                assert_never(unreachable)

    @staticmethod
    def _written(
        row: FormatRow, value: BaseCloudEvent | Block[BaseCloudEvent], /
    ) -> RuntimeRail[Encoded]:
        if isinstance(value, Block):
            return row.batch_codec.to_result_with(lambda: EVENT_FORMAT.raised(row.suffix, "batch")).bind(
                lambda codec: boundary(
                    EVENT_ENCODE,
                    lambda: Encoded(media=f"{BATCH_STEM}{row.suffix}", body=codec.write(value)),
                    catch=codec.raises,
                )
            )
        return boundary(
            EVENT_ENCODE,
            lambda: Encoded(media=row.structured, body=row.codec.write(value)),
            catch=_FORMAT_RAISES,
        )

    def _row(self, suffix: Suffix, mode: str, /) -> RuntimeRail[FormatRow]:
        return self.rows.try_find(suffix).to_result_with(lambda: EVENT_FORMAT.raised(suffix, mode))

    @staticmethod
    def _payloads(
        row: FormatRow, value: MessageEnvelope | Block[MessageEnvelope], /
    ) -> RuntimeRail[None]:
        envelopes = Block.singleton(value) if isinstance(value, MessageEnvelope) else value
        refused = envelopes.filter(lambda envelope: _payload_arm(envelope.payload) not in row.payloads)
        return (
            Ok(None)
            if refused.is_empty()
            else Error(EVENT_FORMAT.raised(row.suffix, _payload_arm(refused.head().payload).value))
        )

    def _framed(self, media: str, /) -> RuntimeRail[tuple[FormatRow, bool]]:
        return boundary(
            EVENT_FORMAT,
            lambda: parse_media(media),
            catch=(TypeError, ValueError),
        ).bind(lambda parsed: self._media_row(parsed, media))

    def _media_row(self, media: MediaType, raw: str, /) -> RuntimeRail[tuple[FormatRow, bool]]:
        if media.maintype != "application":
            return Error(EVENT_FORMAT.raised(raw, "media"))
        batch = media.subtype.startswith("cloudevents-batch+")
        stem = "cloudevents-batch+" if batch else "cloudevents+"
        suffix = media.subtype.removeprefix(stem)
        if media.subtype == suffix or suffix not in ("json", "protobuf", "avro"):
            return Error(EVENT_FORMAT.raised(raw, "media"))
        return self._row(cast(Suffix, suffix), "batch" if batch else "structured").bind(
            lambda row: Ok((row, batch))
            if not batch or row.batch_codec.is_some()
            else Error(EVENT_FORMAT.raised(row.suffix, "batch"))
        )


# --- [OPERATIONS] -----------------------------------------------------------------------


def parse_media(raw: str, /) -> MediaType:
    if not raw or "\r" in raw or "\n" in raw:
        raise ValueError("invalid media type")
    parsed = HeaderParser(policy=policy.default).parsestr(f"Content-Type: {raw}\n\n")
    header = parsed["content-type"]
    if header is None or header.defects:
        raise ValueError("invalid media type")
    return MediaType(
        maintype=header.maintype.lower(),
        subtype=header.subtype.lower(),
        parameters=tuple((name.lower(), value) for name, value in header.params.items()),
    )


def _format_rows(schema: DictSchema, /) -> Map[Suffix, FormatRow]:
    json_codec = JSONFormat()
    protobuf_codec = ProtobufFormat()
    avro_codec = AvroFormat(_schema=schema)
    rows = (
        FormatRow(
            "json",
            codec=json_codec,
            batch_codec=Some(
                BatchCodec(
                    write=lambda events: _json_batch_write(json_codec, events),
                    read=lambda body: _json_batch_read(json_codec, body),
                    raises=_FORMAT_RAISES,
                )
            ),
            payloads=frozenset({PayloadArm.OPAQUE}),
            fits="the mandatory interoperable default, including the specification JSON array batch form",
            admit="the package JSONFormat for each event; the branch adds only the specification's array framing",
            lifetime="the call; no parsed document or encoder state survives the answered bytes",
            degrade=("binary-valued data expands under data_base64 in structured mode",),
        ),
        FormatRow(
            "protobuf",
            codec=protobuf_codec,
            batch_codec=Some(
                BatchCodec(write=ProtobufFormat.batch, read=ProtobufFormat.unbatch, raises=_FORMAT_RAISES)
            ),
            payloads=frozenset({PayloadArm.OPAQUE, PayloadArm.MESSAGE}),
            fits="generated payloads and typed attributes over the publisher's vendored CloudEvent messages",
            admit="the exact generated CloudEvent and CloudEventBatch bindings; Any.type_url remains payload identity",
            lifetime="the call; generated values are rebuilt from the corpus and no descriptor mirror is retained here",
            degrade=("an unseated Any type URL refuses at the separately configured payload registry",),
        ),
        FormatRow(
            "avro",
            codec=avro_codec,
            batch_codec=Nothing,
            payloads=frozenset({PayloadArm.OPAQUE}),
            fits="a singular CloudEvent encoded against the publisher's exact frozen Avro event schema",
            admit="fastavro schemaless read/write over the generated rasm.contracts publisher resource",
            lifetime="the EventFormat composition; the parsed schema is immutable instance state and is parsed once",
            degrade=("the CloudEvents Avro format defines no batch representation",),
        ),
    )
    return Map.of_seq((row.suffix, row) for row in rows)


def _json_batch_write(codec: JSONFormat, events: Block[BaseCloudEvent], /) -> bytes:
    return b"[" + b",".join(codec.write(event) for event in events) + b"]"


def _json_batch_read(codec: JSONFormat, body: bytes, /) -> Block[BaseCloudEvent]:
    members = msgspec.json.decode(body, type=list[Raw])
    return Block.of_seq(codec.read(CloudEvent, bytes(member)) for member in members)


def _admitted_events(events: Block[BaseCloudEvent], /) -> RuntimeRail[Decoded]:
    return traverse(MessageEnvelope.decoded, events).map(
        lambda admitted_events: Decoded(
            events=admitted_events.map(lambda admitted: admitted[0]),
            ignored=admitted_events.collect(lambda admitted: admitted[1]),
        )
    )


def _strict_events(
    value: BaseCloudEvent | Block[BaseCloudEvent], /
) -> RuntimeRail[BaseCloudEvent | Block[BaseCloudEvent]]:
    if isinstance(value, Block):
        return traverse(_strict, value)
    return _strict(value)


def _payload_bytes(data: Payload, /) -> bytes:
    match data:
        case Message() as message:
            return message.to_binary()
        case bytes() | bytearray():
            return bytes(data)
        case str():
            return data.encode()
        case dict():
            return msgspec.json.encode(data)
        case None:
            return b""
        case _ as unreachable:
            assert_never(unreachable)


def _payload_arm(data: EventData, /) -> PayloadArm:
    return PayloadArm.MESSAGE if isinstance(data, Message) else PayloadArm.OPAQUE


def _avro_attribute(value: object, /) -> bool | int | str | bytes:
    match value:
        case bool() | str() | bytes() as scalar:
            return scalar
        case int() as integer:
            return _ce_integer(integer)
        case datetime() as stamp:
            return stamp.isoformat().replace("+00:00", "Z")
        case _:
            raise TypeError(type(value).__name__)


def _avro_data_encode(value: object, /) -> object:
    match value:
        case bytes() | bytearray():
            return bytes(value)
        case None | bool() | str():
            return value
        case (int() | float()) as number if not isinstance(number, bool):
            return _avro_number(number)
        case dict() as record:
            return _avro_record_encode(record)
        case list() as records:
            return [{"value": _avro_record_encode(_avro_object(record))} for record in records]
        case _:
            raise TypeError(type(value).__name__)


def _avro_data_decode(value: object, /) -> object:
    match value:
        case list() as records:
            return [_avro_record_decode(_avro_value(record)) for record in records]
        case dict() as record:
            return _avro_record_decode(record)
        case _:
            return value


def _avro_record_encode(record: dict[object, object], /) -> dict[str, object]:
    encoded: dict[str, object] = {}
    for key, value in record.items():
        if not isinstance(key, str):
            raise TypeError("CloudEvents Avro objects require string keys")
        encoded[key] = _avro_nested_encode(value)
    return encoded


def _avro_nested_encode(value: object, /) -> object:
    match value:
        case None | bool() | str():
            return value
        case (int() | float()) as number if not isinstance(number, bool):
            return _avro_number(number)
        case dict() as record:
            return {"value": _avro_record_encode(record)}
        case list() as records:
            return [{"value": _avro_record_encode(_avro_object(record))} for record in records]
        case _:
            raise TypeError(type(value).__name__)


def _avro_record_decode(record: dict[object, object], /) -> dict[str, object]:
    decoded: dict[str, object] = {}
    for key, value in record.items():
        if not isinstance(key, str):
            raise TypeError("AvroCloudEventData keys must be strings")
        decoded[key] = _avro_nested_decode(value)
    return decoded


def _avro_nested_decode(value: object, /) -> object:
    match value:
        case list() as records:
            return [_avro_record_decode(_avro_value(record)) for record in records]
        case dict() as wrapped:
            return _avro_record_decode(_avro_value(wrapped))
        case _:
            return value


def _avro_object(value: object, /) -> dict[object, object]:
    if isinstance(value, dict):
        return value
    raise TypeError("CloudEvents Avro arrays contain JSON objects")


def _avro_number(value: int | float, /) -> float:
    encoded = float(value)
    if not isfinite(encoded) or isinstance(value, int) and int(encoded) != value:
        raise OverflowError("CloudEvents Avro number is not exactly representable as double")
    return encoded


def _avro_value(value: object, /) -> dict[object, object]:
    wrapped = _avro_object(value)
    return _avro_object(wrapped.get("value"))


def _attribute(name: str, value: object) -> cloudevents_pb.CloudEvent.CloudEventAttributeValue:
    if name in EXTENSION_ROWS:
        return _rostered_attribute(EXTENSION_ROWS[name], value)
    if name == "dataschema" and isinstance(value, str):
        return cloudevents_pb.CloudEvent.CloudEventAttributeValue(attr=Oneof("ce_uri", value))
    match value:
        case CloudEventUri():
            return cloudevents_pb.CloudEvent.CloudEventAttributeValue(attr=Oneof("ce_uri", str(value)))
        case CloudEventUriRef():
            return cloudevents_pb.CloudEvent.CloudEventAttributeValue(attr=Oneof("ce_uri_ref", str(value)))
        case bool():
            return cloudevents_pb.CloudEvent.CloudEventAttributeValue(attr=Oneof("ce_boolean", value))
        case int():
            return cloudevents_pb.CloudEvent.CloudEventAttributeValue(attr=Oneof("ce_integer", _ce_integer(value)))
        case bytes():
            return cloudevents_pb.CloudEvent.CloudEventAttributeValue(attr=Oneof("ce_bytes", value))
        case datetime():
            return cloudevents_pb.CloudEvent.CloudEventAttributeValue(
                attr=Oneof("ce_timestamp", wkt.Timestamp.from_datetime(value))
            )
        case str():
            return cloudevents_pb.CloudEvent.CloudEventAttributeValue(attr=Oneof("ce_string", value))
        case other:
            raise TypeError(type(other).__name__)


def _rostered_attribute(
    row: Codec[Any], value: object, /
) -> cloudevents_pb.CloudEvent.CloudEventAttributeValue:
    match row.arm, value:
        case "ce_integer", int() as held if not isinstance(held, bool):
            return cloudevents_pb.CloudEvent.CloudEventAttributeValue(
                attr=Oneof("ce_integer", _ce_integer(held))
            )
        case "ce_string", str() as held:
            return cloudevents_pb.CloudEvent.CloudEventAttributeValue(attr=Oneof("ce_string", held))
        case "ce_uri_ref", str() as held:
            return cloudevents_pb.CloudEvent.CloudEventAttributeValue(attr=Oneof("ce_uri_ref", held))
        case "ce_timestamp", datetime() as held:
            return cloudevents_pb.CloudEvent.CloudEventAttributeValue(
                attr=Oneof("ce_timestamp", wkt.Timestamp.from_datetime(held))
            )
        case _:
            raise TypeError(f"{row.arm}:{type(value).__name__}")


def _wired(event: BaseCloudEvent) -> cloudevents_pb.CloudEvent:
    attributes = dict(event.get_attributes())
    data = None
    match event.get_data():
        case wkt.Any() as packed:
            data = Oneof("proto_data", packed)
        case Message() as message:
            data = Oneof("proto_data", wkt.Any.pack(message))
        case bytes() | bytearray() as octets:
            data = Oneof("binary_data", bytes(octets))
        case str() as text:
            data = Oneof("text_data", text)
        case dict():
            raise TypeError("protobuf structured data must be bytes, text, or a generated message")
        case None:
            data = None
        case _ as unreachable:
            assert_never(unreachable)
    return cloudevents_pb.CloudEvent(
        id=str(attributes["id"]),
        source=str(attributes["source"]),
        spec_version=str(attributes["specversion"]),
        type=str(attributes["type"]),
        attributes={
            name: _attribute(name, value)
            for name, value in attributes.items()
            if name not in _ENVELOPE_FIELDS
        },
        data=data,
    )


def _attributes(wire: cloudevents_pb.CloudEvent) -> dict[str, Any]:
    mapped = {name: _attribute_value(name, cell) for name, cell in wire.attributes.items()}
    return {"id": wire.id, "source": wire.source, "specversion": wire.spec_version, "type": wire.type} | mapped


def _attribute_value(
    name: str, cell: cloudevents_pb.CloudEvent.CloudEventAttributeValue, /
) -> object:
    arm = cell.attr
    if arm is None:
        raise ValueError(f"{name}:unset-attribute-arm")
    expected = (
        EXTENSION_ROWS[name].arm
        if name in EXTENSION_ROWS
        else {
            "datacontenttype": "ce_string",
            "dataschema": "ce_uri",
            "subject": "ce_string",
            "time": "ce_timestamp",
        }.get(name)
    )
    if expected is not None and arm.field != expected:
        raise TypeError(f"{name}:{arm.field}:{expected}")
    match arm:
        case Oneof(field="ce_timestamp", value=stamp):
            return stamp.to_datetime()
        case Oneof(field="ce_uri", value=uri):
            return CloudEventUri(uri)
        case Oneof(field="ce_uri_ref", value=reference):
            return CloudEventUriRef(reference)
        case _:
            return arm.value


def _payload(wire: cloudevents_pb.CloudEvent) -> Payload:
    match wire.data:
        case None:
            return None
        case Oneof(field="proto_data", value=packed):
            return packed
        case Oneof(field="binary_data", value=octets):
            return octets
        case Oneof(field="text_data", value=text):
            return text
        case _ as unreachable:
            assert_never(unreachable)
```

## [05]-[RESEARCH]

(none)

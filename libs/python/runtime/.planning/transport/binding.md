# [PY_RUNTIME_BINDING]

Every protocol lowering of a message envelope seats here: `BINDINGS` is the one row family carrying content mode, header prefix, routing key, `protocolsettings` slice, filter pushdown, execution arm, and payload-store policy per protocol, and `Emitter` is the `observe` subscription that turns a fired hook fact into a message envelope and hands it to a bound binding. Rows span HTTP, Kafka, both MQTT protocol versions, AMQP 1.0, NATS, and RabbitMQ; growth is one row and every consumer stands untouched, because binding is DATA and never a type a caller switches on.

Specification law owns each lowering and the SDK accelerates four of them: the four SDK binding modules carry distinct prefix families and MQTT carries none, so the MQTT and NATS rows are branch-owned whole. Composed owners: `transport/event#MESSAGE` the message envelope, `transport/event#FORMAT` the codec, `transport/roots#STORE` the `dataref` store, `reliability/resilience#RESILIENCE` every curve, window, and rate, `observability/hooks#HOOKS` the fired facts, `observability/journal#FACT` the durable write, `execution/admission#CONTEXT` the profile, principal scope, and tenancy adoption.

`BrokerLane` closes the connection half the rows describe: one owner drives every protocol's membership, settlement, transactional boundary, poll cadence, and drain off the same `BINDINGS` row that lowers its carrier and a composition-bound `Client` port. A seventh protocol is one row beside one bound port, with no adapter class, arm, or second authority to add. No package on this page creates or owns an event loop — every lane composes inside the caller's `anyio` task group and `lifecycle` defaults `caller-owned`.

## [01]-[INDEX]

- [02]-[BINDING]: binding rows — content modes, the prefix families, routing keys, `protocolsettings`, pushdown verdicts, execution arms, and the per-binding `dataref` policy.
- [03]-[EMISSION]: `Emitter` — the hook subscription, its per-event batch settlement, and the declared paths every long-tail state crosses.
- [04]-[ADAPTER]: `BrokerLane` — the per-protocol execution arm, group membership, the journal-joined settlement, transactional boundaries, poll-loop lifetime, backpressure, dead-letter routing, and the flushing drain.

## [02]-[BINDING]

- Owner: `BindingRow` is the branch's whole protocol vocabulary and `BINDINGS` the table every consumer reads — a lowering, a subscription's `protocolsettings` slice, a filter's pushdown verdict, an execution arm, and a payload threshold all derive from one row, so a new protocol is ONE row and no fold, adapter, or admission gate is edited. `Content` closes the mode vocabulary and `Prefix` the header family, both spelled once rather than as literals at four lowering sites.
- Cases: distinct prefix families span the SDK bindings — `ce-` for HTTP and RabbitMQ, `ce_` for Kafka, `cloudEvents_` for AMQP writing and `cloudEvents:` for AMQP reading — while MQTT carries NO prefix at all, its attributes riding bare User Property names, and NATS carries `ce-` under a branch-owned lowering the distribution does not ship. Reading `ce-` as one repo-wide spelling is the drift this table forecloses.
- Cases: mode and structured-format support are per row. HTTP, Kafka, MQTT 5, AMQP 1.0, NATS, and RabbitMQ carry binary and singular structured formats; MQTT 3.1.1 carries JSON structured only, whose media type is implied because that protocol has no Content Type property. Generic JSON/protobuf batch codecs remain at the format owner, while this connection owner declines batch send until a bounded producer can return per-event custody. Unsupported combinations refuse at admission and never lower another way.
- Law: `protocolsettings` is the subscription's own per-binding slice and REPLACES every hand-rolled per-sink knob — MQTT `topicname` required beside `qos`/`retain`/`expiry`/`userproperties`; AMQP `address`/`linkname`/`sendersettlementmode`/`linkproperties`; Kafka `topicname`/`partitionkeyextractor`/`clientid`/`acks`; NATS `subject`; RabbitMQ the branch's own `exchange`/`routingkey`/`deliverymode`/`expiration`, since the specification carries no RabbitMQ entry. A dual-store row admits `datarefprojection=reference` only as explicit peer-negotiation evidence; without it the row refuses rather than weakening carriage. HTTP request method, destination, authorization, and application headers remain on its bound client/target policy; this carrier row invents none. Unknown keys refuse before any codec or client sees them, while the complete admitted slice reaches the bound dialer.
- Law: pushdown is a row verdict, never a runtime probe. MQTT resolves a topic filter at the BROKER through SUBSCRIBE, NATS through subject wildcards, RabbitMQ through the exchange binding on its routing key, and AMQP through link-source filters under a `copy` or `move` distribution mode; Kafka has no server-side filtering and HTTP no native mechanism, so both filter consumer-side. `transport/filter#DIALECT` owns the dialect half of that join and reads THIS column rather than carrying one of its own — a composite pushes only where every child does, and negation and `sql` never do.
- Law: routing keys are the row's, derived from the roster rather than restated — Kafka takes `partitionkey` onto the record key through the SDK's own `_default_key_mapper`, MQTT and NATS take the topic and subject, RabbitMQ the routing key, and HTTP and AMQP take none. Hand-spelled key extractors beside the helper that owns the roster are the deleted form.
- Law: NO row owns `retry`, and no row carries a column for it either. Transport families foreclose the coordinate and `reliability/resilience#RESILIENCE` holds every schedule, so the answer is uniform across the whole family and rides this line rather than a cell each row re-answers — the adapter that owns a connection binds the class, and a row carrying its own curve makes the effective attempts a product of two.
- Law: tenancy is NOT a row column, on `transport/roots#RESOURCE`'s own reason — a protocol name isolates no tenant. The admitted profile decides the tenancy shape and the application-owned binder supplies an authenticated `PrincipalScope` per received delivery; the generic lane never derives scope from a resolved credential. A coordinate a row cannot express records the divergence on `degrade` rather than dropping the column.
- Law: the execution arm rides the row because it is a protocol fact, and NO row creates or owns an event loop. `KAFKA` is a blocking librdkafka client whose every blocking call releases the GIL, so it rides a `CapacityLimiter`-bounded `to_thread` lane with its delivery, rebalance, and settlement callbacks re-entering through one `BlockingPortalProvider`. `RABBITMQ` is blocking and single-threaded by contract, so it takes one dedicated worker per connection whose only inbound door is `add_callback_threadsafe`, and its pump calls `process_data_events` on its own cadence because deliveries and heartbeats dispatch nowhere else. `MQTT` needs no thread at all — `socket()` registers on the caller's own readiness and `loop_read`/`loop_write`/`loop_misc` run as bounded steps inside the task group. `NATS` is asyncio-native and composes directly, forfeiting the trio backend on its own `degrade`. `HTTP` rides the `transport/roots#RESOURCE` arm already bound.
- Law: `dataref` is ONE policy row per binding and never a global constant, because a threshold fixed repo-wide either strands the smallest transport or wastes the largest. `threshold` comes from the live client ceiling where the protocol negotiates one — NATS `max_payload`, MQTT 5.0 `Maximum Packet Size`, AMQP link `max-frame-size` — and from the row floor otherwise. `PayloadStore` binds the existing `transport/roots#STORE` `ResourceRoot`/`ObjectStoreLane` at composition, persists the exact event-data bytes under the subject content key with the row's `Retain` tag, and resolves the admitted URI-reference through that confined root. Its mandatory `DataIdentity` table binds each event type to the existing content-identity policy and seed, so acquired bytes must prove the envelope's `subject`; dual carriage additionally proves byte equality. An unbound store refuses before any reference ships. `CarriageMode` records dual versus reference-only carriage. A row declaring reference-only projects once past its threshold. A dual row never silently changes semantics: beyond its live ceiling it refuses until admitted `protocolsettings` carry a real negotiation proof. `Externalized` carries the reference, retention, projection, and byte count.
- Law: `dataclassification` gates what crosses which binding, and `CLASSIFICATION_ROWS` is where that gate is DATA — the generated extension carries the canonical string and admission resolves it through `Classification(value)` before table lookup. Missing and unknown values both refuse before lowering, trust, routing, or settlement; no transport arm fabricates an `internal` grade. A grade a binding cannot honor refuses before encoding. Transport never mutates a typed payload under a generic "redaction" label; `SECRET` reaches no binding at all, and application projection owns any sanctioned lower-sensitivity fact.
- Entry: `lower(value, binding, mode, suffix, settings, formats)` is one entry over every protocol: the row admits the mode, binary requires no suffix and one envelope, structured requires one envelope plus a suffix, and batch requires a block plus a suffix. The composition-bound `EventFormat` owns bytes; this owner lowers only the carrier. `raise_(message, binding, formats)` derives structured/batch from media type and otherwise delegates binary attributes to the official SDK before the same generated-profile admission. `PayloadStore.resolve(envelope)` is the inverse data leg, admitting and confining `dataref`, acquiring its bytes, verifying `subject`, and comparing dual data before returning.
- Auto: a binding a deployment cannot serve refuses on the `providers` OPEN axis as one `execution/admission#CONTEXT` descriptor row, never a boolean knob, because a knob re-mints the assumed consumer roster the open form forecloses. AMQP 1.0 is exactly that case in this branch: the row lowers and raises an `AMQPMessage` value and names no client, so a composition binding it refuses at admission with the axis named.
- Growth: a new protocol is one `BindingRow` with its `Dataref` row, reaching every `CLASSIFICATION_ROWS` `broker` cell that admits it; a new sensitivity grade is one `Classification` member at its admission owner with one `CLASSIFICATION_ROWS` row here; a new content mode is one `Content` member on the rows that hold it; a new protocol setting is one key on that row's slice; a new pushdown mechanism is one `Pushdown` value; a new execution arm is one `Arm` member with its lane law; a new store is one port binding at the composition root.
- Boundary: protocol lowering, its policy rows, and payload store only — the connection half seats at `[04]-[ADAPTER]` on this same page, so a row states the protocol fact and the lane realizes it. Composes — never re-mints — the message envelope, the format contract, the resilience curves, the store lane, and the hook registry. Rejected: a per-sink knob outside its row's `protocolsettings` slice; a `ce-` literal at a lowering site; a hand-spelled partition-key extractor beside `_default_key_mapper`; a global `dataref` threshold; a `retry` column on a transport row; a boolean capability knob where the `providers` axis refuses.

```python
# --- [IMPORTS] --------------------------------------------------------------------------
from collections.abc import Iterable
from copy import replace
from enum import StrEnum
from hmac import compare_digest
from typing import Final, Literal, Never, assert_never, cast

from expression import Error, Nothing, Ok, Option, Result, Some
from expression.collections import Block, Map
from msgspec import Raw, Struct
from obstore import Bytes
from protobuf import Message as ProtoMessage

from cloudevents.core.bindings import amqp, http, kafka, rabbitmq
from cloudevents.core.bindings.common import encode_header_value
from cloudevents.core.bindings.kafka import PARTITIONKEY_ATTR
from cloudevents.core.exceptions import CloudEventValidationError
from cloudevents.core.formats.base import Format
from cloudevents.core.v1.event import CloudEvent

from rasm.runtime.admission import Classification
from rasm.runtime.event import (
    Content,
    Decoded,
    Encoded,
    EventFormat,
    EventType,
    MediaType,
    MessageEnvelope,
    Suffix,
    WireKey,
    parse_media,
)
from rasm.runtime.faults import BINDING_ADMIT, BINDING_DECODE, BINDING_ENCODE, RuntimeResult, boundary
from rasm.runtime.identity import CANONICAL_POLICY, ContentIdentity, IdentityPolicy, U64
from rasm.runtime.journal import Retain
from rasm.runtime.roots import ObjectStoreLane, ResourceRef, ResourceRoot, StoreOp, StoreOutcome

# --- [TYPES] ----------------------------------------------------------------------------

type Message = http.HTTPMessage | kafka.KafkaMessage | amqp.AMQPMessage | rabbitmq.RabbitMQMessage | MqttMessage | NatsMessage
type Settings = Map[str, str]


class MqttMessage(Struct, frozen=True, gc=False):
    topic: str
    properties: tuple[tuple[str, str], ...]
    content_type: Option[str]
    payload: bytes


class NatsMessage(Struct, frozen=True, gc=False):
    subject: str
    headers: Map[str, str]
    payload: bytes

# --- [CONSTANTS] ------------------------------------------------------------------------


class Binding(StrEnum):
    HTTP = "http"
    KAFKA = "kafka"
    MQTT5 = "mqtt5"
    MQTT311 = "mqtt311"
    AMQP = "amqp"
    NATS = "nats"
    RABBITMQ = "rabbitmq"


class Prefix(StrEnum):
    DASH = "ce-"
    UNDERSCORE = "ce_"
    QUALIFIED = "cloudEvents_"
    NONE = ""


class Arm(StrEnum):
    THREAD = "thread"
    PUMP = "pump"
    READY = "ready"
    NATIVE = "native"
    ABSENT = "absent"


class Pushdown(StrEnum):
    BROKER = "broker"
    LINK = "link"
    CONSUMER = "consumer"


class CarriageMode(StrEnum):
    DUAL = "dual"
    REFERENCE = "reference"


class Pump(StrEnum):
    ABSENT = "absent"
    POLL = "poll"
    WORKER = "worker"
    READY = "ready"
    NATIVE = "native"
    REQUEST = "request"


class Grouping(StrEnum):
    NONE = "none"
    GROUP = "group"
    QUEUE = "queue"
    WORK = "work"


class Settle(StrEnum):
    JOURNAL = "journal"
    BROKER = "broker"
    RESPONSE = "response"


class Producing(StrEnum):
    TRANSACTIONAL = "transactional"
    IDEMPOTENT = "idempotent"
    CONFIRMED = "confirmed"
    UNCONFIRMED = "unconfirmed"


# --- [MODELS] ---------------------------------------------------------------------------


class ClassificationRow(Struct, frozen=True, gc=False):
    grade: Classification
    broker: frozenset[Binding]
    carries: str


CLASSIFICATION_ROWS: Final[Map[Classification, ClassificationRow]] = Map.of_seq(
    (row.grade, row)
    for row in (
        ClassificationRow(
            Classification.PUBLIC,
            broker=frozenset(Binding),
            carries="a fact whose payload is publishable as it stands, so every binding carries it",
        ),
        ClassificationRow(
            Classification.INTERNAL,
            broker=frozenset(Binding),
            carries="an explicitly classified repo-interior fact every admitted binding carries under the standing trust row",
        ),
        ClassificationRow(
            Classification.RESTRICTED,
            broker=frozenset({Binding.HTTP, Binding.KAFKA, Binding.RABBITMQ}),
            carries="a fact only the bindings with a verified destination trust row may carry",
        ),
        ClassificationRow(
            Classification.SECRET,
            broker=frozenset(),
            carries="a fact no broker carries at all; transport never strips its payload into a weaker event",
        ),
    )
)


class Dataref(Struct, frozen=True, gc=False):
    threshold: int
    negotiated: bool
    retain: Retain
    projection: CarriageMode


class Externalized(Struct, frozen=True, gc=False):
    ref: ResourceRef
    retain: Retain
    projection: CarriageMode
    quantity: int


class DataIdentityRow(Struct, frozen=True, gc=False):
    fmt: str
    policy: IdentityPolicy = CANONICAL_POLICY
    seed: Option[U64] = Nothing


class DataIdentity(Struct, frozen=True, gc=False):
    rows: Map[EventType, DataIdentityRow]

    def verify(self, event_type: EventType, expected: WireKey, body: bytes, /) -> RuntimeResult[None]:
        return self.rows.try_find(event_type).to_result_with(
            lambda: BINDING_DECODE.raised("store", f"missing-identity:{event_type.wire}")
        ).bind(
            lambda row: ContentIdentity.of(
                row.fmt,
                body,
                row.policy,
                view="wire",
                seed=row.seed,
            ).bind(
                lambda actual: Ok(None)
                if compare_digest(actual, expected)
                else Error(BINDING_DECODE.raised("store", "subject-mismatch"))
            )
        )


class PayloadStore(Struct, frozen=True, gc=False):
    root: ResourceRoot
    identity: DataIdentity

    async def externalize(
        self, envelope: MessageEnvelope, policy: Dataref, projection: CarriageMode, /
    ) -> RuntimeResult[tuple[MessageEnvelope, Externalized]]:
        match envelope.subject:
            case Option(tag="none"):
                return Error(BINDING_ADMIT.raised("store", "dataref-without-subject"))
            case Option(tag="some", some=subject):
                referred = self.root.child(f"events/{subject}.payload")
        match referred:
            case Result(tag="error") as refused:
                return refused
            case Result(tag="ok", ok=ref):
                payload = _event_data(envelope)
        match payload:
            case Result(tag="error") as refused:
                return refused
            case Result(tag="ok", ok=body):
                projected = replace(
                    envelope,
                    payload=envelope.payload if projection is CarriageMode.DUAL else None,
                    extensions=replace(envelope.extensions, dataref=ref.relative),
                )
        match projected.event():
            case Result(tag="error") as refused:
                return refused
            case Result(tag="ok"):
                stored = await ObjectStoreLane.of(ref).run_async(StoreOp.Put(body, tags={"retain": policy.retain.value}))
        return stored.bind(
            lambda outcome: _stored(outcome, len(body)).map(
                lambda quantity: (
                    projected,
                    Externalized(
                        ref=ref,
                        retain=policy.retain,
                        projection=projection,
                        quantity=quantity,
                    ),
                )
            )
        )

    async def resolve(self, envelope: MessageEnvelope, /) -> RuntimeResult[bytes]:
        if not envelope.extensions.has_field("dataref"):
            return Error(BINDING_DECODE.raised("store", "missing-dataref"))
        match envelope.subject:
            case Option(tag="none"):
                return Error(BINDING_DECODE.raised("store", "dataref-without-subject"))
            case Option(tag="some", some=subject):
                referred = self.root.child(envelope.extensions.dataref)
        match referred:
            case Result(tag="error") as refused:
                return refused
            case Result(tag="ok", ok=ref):
                acquired = (await ObjectStoreLane.of(ref).run_async(StoreOp.Get())).bind(_resolved)
        return acquired.bind(
            lambda body: self.identity.verify(envelope.event_type, subject, body)
            .bind(lambda _: _same_data(envelope, body))
            .map(lambda _: body)
        )


class BindingRow(Struct, frozen=True, gc=False):
    binding: Binding
    modes: frozenset[Content]
    formats: frozenset[Suffix]
    prefix: Prefix
    routes_on: Option[str]
    settings: frozenset[str]
    pushdown: Pushdown
    arm: Arm
    dataref: Dataref
    pump: Pump
    grouping: Grouping
    settlement: Settle
    producing: Producing
    lane: Option[str]
    portal: bool
    rebalanced: bool
    prefetch: int
    fits: str
    admit: str
    lifetime: str
    deliver: str
    order: str
    settle: str
    replay: str
    bound: str
    refuse: str
    degrade: tuple[str, ...]


BINDINGS: Final[Map[Binding, BindingRow]] = Map.of_seq(
    (row.binding, row)
    for row in (
        BindingRow(
            Binding.HTTP,
            modes=frozenset({Content.BINARY, Content.STRUCTURED}),
            formats=frozenset({"json", "protobuf", "avro"}),
            prefix=Prefix.DASH,
            routes_on=Nothing,
            settings=frozenset(),
            pushdown=Pushdown.CONSUMER,
            arm=Arm.NATIVE,
            dataref=Dataref(
                threshold=8 << 10,
                negotiated=False,
                retain=Retain.OPERATIONAL,
                projection=CarriageMode.REFERENCE,
            ),
            pump=Pump.REQUEST,
            grouping=Grouping.NONE,
            settlement=Settle.RESPONSE,
            producing=Producing.CONFIRMED,
            lane=Nothing,
            portal=False,
            rebalanced=False,
            prefetch=0,
            fits="an application-bound HTTP ingress or egress carrier whose request and response lifetime another owner holds",
            admit="`lower` over `cloudevents.core.bindings.http`, dialed through the `transport/roots#RESOURCE` http arm already bound",
            lifetime="the request; nothing survives the response and no subscription state accumulates here",
            deliver="at-most-once on a bare POST, at-least-once where the target answers and the producer re-drives",
            order="none across requests; `sequence` crosses as producer metadata and establishes no transport order",
            settle="the response status IS the settlement, so a 2xx is the whole acknowledgement",
            replay="none; the producer re-issues or the fact is lost",
            bound="the server's own header budget, which is why binary mode is the constrained one here",
            refuse="a classification the target's trust row does not carry, and an attribute set past the header budget",
            degrade=(
                "binary mode puts every attribute in a header against an 8-16 KiB server budget, so a wide extension roster forces structured mode",
                "no server-side filtering exists at all, so every subscription expression evaluates after delivery",
                "non-ASCII attribute values percent-encode over a narrower safe set than the shared encoder uses",
            ),
        ),
        BindingRow(
            Binding.KAFKA,
            modes=frozenset({Content.BINARY, Content.STRUCTURED}),
            formats=frozenset({"json", "protobuf", "avro"}),
            prefix=Prefix.UNDERSCORE,
            routes_on=Some(PARTITIONKEY_ATTR),
            settings=frozenset({"topicname", "partitionkeyextractor", "clientid", "acks", "datarefprojection"}),
            pushdown=Pushdown.CONSUMER,
            arm=Arm.THREAD,
            dataref=Dataref(
                threshold=1 << 20,
                negotiated=False,
                retain=Retain.OPERATIONAL,
                projection=CarriageMode.DUAL,
            ),
            pump=Pump.POLL,
            grouping=Grouping.GROUP,
            settlement=Settle.JOURNAL,
            producing=Producing.TRANSACTIONAL,
            lane=Some("broker.kafka"),
            portal=True,
            rebalanced=True,
            prefetch=1000,
            fits="the durable, partitioned, replayable log every analytic and audit consumer reads, and the one binding carrying a registry-framed payload",
            admit="`lower` over `cloudevents.core.bindings.kafka`, produced through the `confluent-kafka` client on a bounded thread lane",
            lifetime="the topic's own retention; this branch commits offsets and deletes nothing",
            deliver="at-least-once by default, exactly-once inside a transaction whose offsets settle with the produce",
            order="total within a partition, so `partitionkey` decides what stays ordered and nothing orders across keys",
            settle="an explicit offset commit joined to the durable write, never an auto-commit that outruns it",
            replay="a seek to any retained offset, which is what makes this the audit-grade row",
            bound="`message.max.bytes` and `max.request.size`, both about a mebibyte",
            refuse="a payload past the frame budget with no bound store, and a classification the topic's trust row does not carry",
            degrade=(
                "no server-side filtering, so every subscription expression evaluates consumer-side after the fetch",
                "headers cross as bytes and every attribute value stringifies, so a typed extension round-trips only through the roster's own codecs",
                "the SDK does not propagate headers onto the message handed to a delivery report, so producer-side evidence reads the envelope it sent",
            ),
        ),
        BindingRow(
            Binding.MQTT5,
            modes=frozenset({Content.BINARY, Content.STRUCTURED}),
            formats=frozenset({"json", "protobuf", "avro"}),
            prefix=Prefix.NONE,
            routes_on=Some("topicname"),
            settings=frozenset({"topicname", "qos", "retain", "expiry", "userproperties", "datarefprojection"}),
            pushdown=Pushdown.BROKER,
            arm=Arm.READY,
            dataref=Dataref(
                threshold=64 << 10,
                negotiated=True,
                retain=Retain.EPHEMERAL,
                projection=CarriageMode.DUAL,
            ),
            pump=Pump.READY,
            grouping=Grouping.QUEUE,
            settlement=Settle.BROKER,
            producing=Producing.CONFIRMED,
            lane=Nothing,
            portal=False,
            rebalanced=False,
            prefetch=20,
            fits="the edge and telemetry plane — many small producers on constrained links, filtered at the broker",
            admit="the branch-owned lowering onto MQTT 5.0 User Properties; the distribution ships no MQTT binding at all",
            lifetime="the session; a retained message outlives it only where the row's `retain` setting says so",
            deliver="QoS 0 at-most-once, QoS 1 at-least-once, QoS 2 exactly-once — the subscription's own setting decides",
            order="per topic within one session, and nothing across a reconnect",
            settle="PUBACK or PUBCOMP by QoS, deferred where manual acknowledgement is armed",
            replay="none beyond the retained message and the session's own queued window",
            bound="the session's negotiated Maximum Packet Size, read off the live connection rather than this floor",
            refuse="a payload past the negotiated packet size with no bound store, and a classification the topic's trust row refuses",
            degrade=(
                "attributes ride UNPREFIXED User Property names, so a peer's own property namespace collides with an attribute name by construction",
                "a wildcard subscription filters on the topic alone, so every non-topic expression still evaluates consumer-side",
                "the client re-raises a callback fault into its own network loop, so the crossing returns its faults rather than letting one kill the pump",
            ),
        ),
        BindingRow(
            Binding.MQTT311,
            modes=frozenset({Content.STRUCTURED}),
            formats=frozenset({"json"}),
            prefix=Prefix.NONE,
            routes_on=Some("topicname"),
            settings=frozenset({"topicname", "qos", "retain", "datarefprojection"}),
            pushdown=Pushdown.BROKER,
            arm=Arm.READY,
            dataref=Dataref(
                threshold=64 << 10,
                negotiated=False,
                retain=Retain.EPHEMERAL,
                projection=CarriageMode.DUAL,
            ),
            pump=Pump.READY,
            grouping=Grouping.NONE,
            settlement=Settle.BROKER,
            producing=Producing.UNCONFIRMED,
            lane=Nothing,
            portal=False,
            rebalanced=False,
            prefetch=20,
            fits="a legacy broker or device fleet that never negotiated 5.0, reached without forking the producer",
            admit="the same branch-owned lowering, structured mode only",
            lifetime="the session, on the 5.0 row's own law",
            deliver="QoS 0, 1, or 2 exactly as the 5.0 row",
            order="per topic within one session",
            settle="PUBACK or PUBCOMP by QoS",
            replay="the retained message alone",
            bound="the broker's own packet ceiling, unnegotiated and therefore this declared floor",
            refuse="every binary-mode request, by name, on this row",
            degrade=(
                "STRUCTURED ONLY: the protocol has no user-property surface, so binary mode is unspellable and every attribute rides inside the body",
                "no message expiry and no reason codes, so an expired fact is dropped by the consumer rather than by the broker",
                "a consumer cannot route on an attribute without decoding the body, which is exactly what the binary mode exists to avoid",
            ),
        ),
        BindingRow(
            Binding.AMQP,
            modes=frozenset({Content.BINARY, Content.STRUCTURED}),
            formats=frozenset({"json", "protobuf", "avro"}),
            prefix=Prefix.QUALIFIED,
            routes_on=Nothing,
            settings=frozenset({"address", "linkname", "sendersettlementmode", "linkproperties", "datarefprojection"}),
            pushdown=Pushdown.LINK,
            arm=Arm.ABSENT,
            dataref=Dataref(
                threshold=128 << 10,
                negotiated=True,
                retain=Retain.OPERATIONAL,
                projection=CarriageMode.DUAL,
            ),
            pump=Pump.ABSENT,
            grouping=Grouping.NONE,
            settlement=Settle.BROKER,
            producing=Producing.UNCONFIRMED,
            lane=Nothing,
            portal=False,
            rebalanced=False,
            prefetch=0,
            fits="an AMQP 1.0 peer this branch must LOWER for without dialing — the value crosses, the connection is another runtime's",
            admit="`lower` over `cloudevents.core.bindings.amqp`, whose value a foreign sender transmits",
            lifetime="the link's, which this branch neither opens nor ends",
            deliver="the link's settlement mode decides, and this branch declares none",
            order="per link, which this branch does not hold",
            settle="the sender settlement mode on the subscription's own slice",
            replay="the peer's, never this branch's",
            bound="the per-link max-frame-size, negotiated by whoever holds the link",
            refuse="every dial: `arm` is ABSENT, so a composition binding this row refuses at admission naming the `providers` axis",
            degrade=(
                "no client at all in this branch, so the row lowers and raises a value and names no connection — the refusal is the honest coordinate, not a gap",
                "the SDK writes `cloudEvents_` and reads both it and `cloudEvents:`, so a peer writing the colon form round-trips while this branch never emits it",
                "this binding alone preserves native bool and int values and stamps `time` as a millisecond epoch integer, so its attribute types differ from every sibling row",
            ),
        ),
        BindingRow(
            Binding.NATS,
            modes=frozenset({Content.BINARY, Content.STRUCTURED}),
            formats=frozenset({"json", "protobuf", "avro"}),
            prefix=Prefix.DASH,
            routes_on=Some("subject"),
            settings=frozenset({"subject", "datarefprojection"}),
            pushdown=Pushdown.BROKER,
            arm=Arm.NATIVE,
            dataref=Dataref(
                threshold=1 << 20,
                negotiated=True,
                retain=Retain.EPHEMERAL,
                projection=CarriageMode.DUAL,
            ),
            pump=Pump.NATIVE,
            grouping=Grouping.QUEUE,
            settlement=Settle.JOURNAL,
            producing=Producing.IDEMPOTENT,
            lane=Nothing,
            portal=False,
            rebalanced=False,
            prefetch=256,
            fits="the low-latency subject-addressed plane, and through JetStream the durable one, reached with no thread lane at all",
            admit="the branch-owned lowering onto NATS headers; the distribution ships no NATS binding",
            lifetime="core NATS holds nothing past delivery; a JetStream stream holds its own declared retention",
            deliver="core NATS at-most-once, JetStream at-least-once under an explicit acknowledgement policy",
            order="per subject on a stream, and none on the core plane",
            settle="the JetStream settlement verbs, and nothing at all on the core plane",
            replay="a JetStream deliver policy over the stream, and none on the core plane",
            bound="the connection's advertised max_payload, read live rather than from this floor",
            refuse="a header-mode request against a server whose INFO does not advertise header support",
            degrade=(
                "the client is asyncio-locked whole, so this row forfeits the trio backend and composes under the asyncio one alone",
                "header support is a SERVER capability read off its own advertisement, so binary mode refuses rather than silently publishing a headerless message",
                "its reader, ping, and flusher legs are loop-level tasks rather than children of the caller's group, so teardown is an explicit drain and not a cancelled scope",
            ),
        ),
        BindingRow(
            Binding.RABBITMQ,
            modes=frozenset({Content.BINARY, Content.STRUCTURED}),
            formats=frozenset({"json", "protobuf", "avro"}),
            prefix=Prefix.DASH,
            routes_on=Some("routingkey"),
            settings=frozenset({"exchange", "routingkey", "deliverymode", "expiration", "datarefprojection"}),
            pushdown=Pushdown.BROKER,
            arm=Arm.PUMP,
            dataref=Dataref(
                threshold=128 << 10,
                negotiated=False,
                retain=Retain.OPERATIONAL,
                projection=CarriageMode.DUAL,
            ),
            pump=Pump.WORKER,
            grouping=Grouping.WORK,
            settlement=Settle.JOURNAL,
            producing=Producing.CONFIRMED,
            lane=Some("broker.rabbitmq"),
            portal=True,
            rebalanced=False,
            prefetch=64,
            fits="a work-queue and routed-fanout plane where the exchange binding does the filtering the consumer would otherwise pay for",
            admit="`lower` over `cloudevents.core.bindings.rabbitmq`, published through `pika`'s blocking channel on its own worker",
            lifetime="the queue's own policy; a durable queue outlives every connection that fed it",
            deliver="at-least-once under publisher confirms, at-most-once without them",
            order="per queue, and nothing across a requeue",
            settle="an explicit ack, nack, or reject carrying its own requeue disposition",
            replay="none; a redelivery is the requeue, not a rewind",
            bound="the negotiated frame max, 128 KiB by this client's default",
            refuse="a classification the exchange's trust row does not carry, and a payload past the frame budget with no bound store",
            degrade=(
                "the client is single-threaded by contract, so one worker owns the connection and every inbound crossing is a threadsafe callback",
                "deliveries, timers, and heartbeats dispatch ONLY inside the pump, so a worker that stops pumping stops answering the broker entirely",
                "the SDK writes plain unencoded header strings here, so a non-ASCII attribute value crosses as whatever the field table encodes rather than percent-encoded",
                "`datacontenttype` rides the dedicated content-type property rather than a header, so it is the one attribute this row does not prefix",
            ),
        ),
    )
)

BROKER_FILTERED: Final[frozenset[Binding]] = frozenset(row.binding for row in BINDINGS.values() if row.pushdown is not Pushdown.CONSUMER)
DIALABLE: Final[frozenset[Binding]] = frozenset(row.binding for row in BINDINGS.values() if row.arm is not Arm.ABSENT)
NEGOTIATED_BOUND: Final[frozenset[Binding]] = frozenset(row.binding for row in BINDINGS.values() if row.dataref.negotiated)
THREADED: Final[frozenset[Binding]] = frozenset(row.binding for row in BINDINGS.values() if row.lane.is_some())
PORTALED: Final[frozenset[Binding]] = frozenset(row.binding for row in BINDINGS.values() if row.portal)
JOURNAL_SETTLED: Final[frozenset[Binding]] = frozenset(
    row.binding for row in BINDINGS.values() if row.settlement is Settle.JOURNAL
)


def _event_data(envelope: MessageEnvelope, /) -> RuntimeResult[bytes]:
    match envelope.payload:
        case Raw() as body:
            return Ok(bytes(body))
        case ProtoMessage() as message:
            return (
                Ok(message.to_binary())
                if envelope.content_type.is_some() and envelope.data_schema.is_some()
                else Error(BINDING_ADMIT.raised("store", "protobuf-data-without-content-coordinate"))
            )
        case None:
            return Error(BINDING_ADMIT.raised("store", "dataref-without-data"))


def _stored(outcome: StoreOutcome, expected: int, /) -> RuntimeResult[int]:
    return (
        Ok(outcome.quantity)
        if outcome.operation == "put" and outcome.quantity == expected
        else Error(BINDING_ENCODE.raised("store", "incomplete-put"))
    )


def _resolved(outcome: StoreOutcome, /) -> RuntimeResult[bytes]:
    return (
        Ok(bytes(outcome.payload))
        if outcome.operation == "get" and isinstance(outcome.payload, (Bytes, bytes, bytearray, memoryview))
        else Error(BINDING_DECODE.raised("store", "non-byte-payload"))
    )


def _same_data(envelope: MessageEnvelope, acquired: bytes, /) -> RuntimeResult[None]:
    match envelope.payload:
        case None:
            return Ok(None)
        case Raw() as inline:
            held = bytes(inline)
        case ProtoMessage() as message:
            held = message.to_binary()
    return (
        Ok(None)
        if compare_digest(held, acquired)
        else Error(BINDING_DECODE.raised("store", "inline-dataref-mismatch"))
    )


def lower(
    value: MessageEnvelope | Block[MessageEnvelope],
    /,
    *,
    binding: Binding,
    mode: Content,
    suffix: Option[Suffix],
    settings: Settings,
    formats: EventFormat,
) -> RuntimeResult[Message]:
    row = BINDINGS[binding]
    admitted_settings = _settings(row, settings)
    if admitted_settings.is_error():
        return admitted_settings
    classified = _classified(value, binding)
    if classified.is_error():
        return classified
    if mode not in row.modes:
        return Error(BINDING_ADMIT.raised(binding.value, mode.value))
    if mode is Content.BINARY:
        if suffix.is_some() or not isinstance(value, MessageEnvelope):
            return Error(BINDING_ADMIT.raised(binding.value, "binary-format-or-batch"))
        return value.event().bind(
            lambda event: boundary(
                BINDING_ENCODE,
                lambda: _binary(event, binding, settings, formats),
                catch=(CloudEventValidationError, TypeError, ValueError, UnicodeError, OverflowError),
            )
        )
    return suffix.to_result_with(lambda: BINDING_ADMIT.raised(binding.value, f"{mode.value}-without-format")).bind(
        lambda selected: Error(BINDING_ADMIT.raised(binding.value, f"{mode.value}-{selected}"))
        if selected not in row.formats
        else _framed(value, mode, selected, binding, settings, formats)
    )


def raise_(message: Message, /, *, binding: Binding, formats: EventFormat) -> RuntimeResult[Decoded]:
    return boundary(
        BINDING_DECODE,
        lambda: _parts(message, binding),
        catch=(TypeError, UnicodeError),
    ).bind(lambda parts: _raised(parts, message, binding, formats))


def _framed(
    value: MessageEnvelope | Block[MessageEnvelope],
    mode: Content,
    suffix: Suffix,
    binding: Binding,
    settings: Settings,
    formats: EventFormat,
    /,
) -> RuntimeResult[Message]:
    if mode is Content.STRUCTURED:
        if not isinstance(value, MessageEnvelope):
            return Error(BINDING_ADMIT.raised(binding.value, "structured-batch"))
        if binding in (Binding.HTTP, Binding.KAFKA, Binding.AMQP, Binding.RABBITMQ):
            return formats.codec(suffix, value).bind(
                lambda codec: value.event().bind(
                    lambda event: boundary(
                        BINDING_ENCODE,
                        lambda: _sdk_structured(event, binding, codec),
                        catch=(CloudEventValidationError, TypeError, ValueError, UnicodeError, OverflowError),
                    )
                )
            )
        return formats.encode(value, suffix=suffix).bind(
            lambda encoded: _carried(encoded, binding, settings)
        )
    if mode is Content.BATCH and not isinstance(value, Block):
        return Error(BINDING_ADMIT.raised(binding.value, "batch-single"))
    return formats.encode(value, suffix=suffix).bind(lambda encoded: _carried(encoded, binding, settings))


def _classified(
    value: MessageEnvelope | Block[MessageEnvelope], binding: Binding, /
) -> RuntimeResult[None]:
    envelopes = Block.singleton(value) if isinstance(value, MessageEnvelope) else value
    return envelopes.fold(
        lambda admitted, envelope: admitted.bind(
            lambda _: _grade(envelope, binding).map(lambda _grade: None)
        ),
        Ok(None),
    )


def _grade(envelope: MessageEnvelope, binding: Binding, /) -> RuntimeResult[Classification]:
    if not envelope.extensions.has_field("dataclassification"):
        return Error(BINDING_ADMIT.raised(binding.value, "missing-dataclassification"))
    return boundary(
        BINDING_ADMIT,
        lambda: Classification(envelope.extensions.dataclassification),
        catch=ValueError,
    ).bind(
        lambda grade: Ok(grade)
        if binding in CLASSIFICATION_ROWS[grade].broker
        else Error(BINDING_ADMIT.raised(binding.value, grade.value))
    )


def _carried(encoded: Encoded, binding: Binding, settings: Settings, /) -> RuntimeResult[Message]:
    match binding:
        case Binding.HTTP:
            return Ok(http.HTTPMessage(headers={"content-type": encoded.media}, body=encoded.body))
        case Binding.MQTT5:
            return _setting(settings, "topicname", binding).map(
                lambda topic: MqttMessage(topic=topic, properties=(), content_type=Some(encoded.media), payload=encoded.body)
            )
        case Binding.MQTT311:
            return _setting(settings, "topicname", binding).map(
                lambda topic: MqttMessage(topic=topic, properties=(), content_type=Nothing, payload=encoded.body)
            )
        case Binding.NATS:
            return _setting(settings, "subject", binding).map(
                lambda subject: NatsMessage(
                    subject=subject,
                    headers=Map.of_seq((("content-type", encoded.media),)),
                    payload=encoded.body,
                )
            )
        case Binding.KAFKA | Binding.AMQP | Binding.RABBITMQ:
            return Error(BINDING_ADMIT.raised(binding.value, "unreachable-manual-structured-carrier"))


def _sdk_structured(event: CloudEvent, binding: Binding, codec: Format, /) -> Message:
    match binding:
        case Binding.HTTP:
            return http.to_structured(event, codec)
        case Binding.KAFKA:
            return kafka.to_structured(event, codec)
        case Binding.AMQP:
            return amqp.to_structured(event, codec)
        case Binding.RABBITMQ:
            return rabbitmq.to_structured(event, codec)
        case Binding.MQTT5 | Binding.MQTT311 | Binding.NATS:
            raise ValueError(f"{binding.value}:no-sdk-structured-binding")


def _binary(event: CloudEvent, binding: Binding, settings: Settings, formats: EventFormat, /) -> Message:
    match binding:
        case Binding.HTTP:
            return http.to_binary(event, formats.payload_codec)
        case Binding.KAFKA:
            return kafka.to_binary(event, formats.payload_codec)
        case Binding.AMQP:
            return amqp.to_binary(event, formats.payload_codec)
        case Binding.RABBITMQ:
            return rabbitmq.to_binary(event, formats.payload_codec)
        case Binding.MQTT5:
            topic = _required_setting(settings, "topicname", binding)
            attributes = event.get_attributes()
            row = BINDINGS[binding]
            properties = tuple(
                (_attribute_name(row, name), encode_header_value(value))
                for name, value in attributes.items()
                if name != "datacontenttype" and value is not None
            )
            return MqttMessage(
                topic=topic,
                properties=properties,
                content_type=Option.of_optional(event.get_datacontenttype()),
                payload=formats.payload_codec.write_data(event.get_data(), event.get_datacontenttype()),
            )
        case Binding.NATS:
            subject = _required_setting(settings, "subject", binding)
            attributes = event.get_attributes()
            row = BINDINGS[binding]
            headers = {
                _attribute_name(row, name): encode_header_value(value)
                for name, value in attributes.items()
                if name != "datacontenttype" and value is not None
            }
            if (content_type := event.get_datacontenttype()) is not None:
                headers["content-type"] = content_type
            return NatsMessage(
                subject=subject,
                headers=Map.of_seq(headers.items()),
                payload=formats.payload_codec.write_data(event.get_data(), content_type),
            )
        case Binding.MQTT311:
            raise ValueError("mqtt311 is structured-only")
        case _ as unreachable:
            assert_never(unreachable)


def _raised_binary(message: Message, binding: Binding, formats: EventFormat, /) -> CloudEvent:
    match binding, message:
        case Binding.HTTP, http.HTTPMessage() as held:
            return cast(CloudEvent, http.from_binary(held, formats.payload_codec, CloudEvent))
        case Binding.KAFKA, kafka.KafkaMessage() as held:
            return cast(CloudEvent, kafka.from_binary(held, formats.payload_codec, CloudEvent))
        case Binding.AMQP, amqp.AMQPMessage() as held:
            return cast(CloudEvent, amqp.from_binary(held, formats.payload_codec, CloudEvent))
        case Binding.RABBITMQ, rabbitmq.RabbitMQMessage() as held:
            return cast(CloudEvent, rabbitmq.from_binary(held, formats.payload_codec, CloudEvent))
        case Binding.MQTT5, MqttMessage() as held:
            headers = _http_headers(held.properties, BINDINGS[binding])
            headers |= {"content-type": value for value in held.content_type.to_list()}
            return cast(CloudEvent, http.from_binary(http.HTTPMessage(headers, held.payload), formats.payload_codec, CloudEvent))
        case Binding.NATS, NatsMessage() as held:
            headers = _http_headers(held.headers.items(), BINDINGS[binding])
            headers |= {key: value for key, value in held.headers.items() if key.lower() == "content-type"}
            return cast(
                CloudEvent,
                http.from_binary(http.HTTPMessage(headers, held.payload), formats.payload_codec, CloudEvent),
            )
        case Binding.MQTT311, MqttMessage():
            raise ValueError("mqtt311 is structured-only")
        case _:
            raise TypeError(f"{binding.value}:{type(message).__name__}")


def _parts(message: Message, binding: Binding, /) -> tuple[str, bytes]:
    match binding, message:
        case Binding.HTTP, http.HTTPMessage() as held:
            return _header(held.headers, "content-type"), held.body
        case Binding.KAFKA, kafka.KafkaMessage() as held:
            return _byte_header(held.headers, "content-type"), held.value
        case Binding.AMQP, amqp.AMQPMessage() as held:
            return str(held.properties.get("content-type", "")), held.application_data
        case Binding.RABBITMQ, rabbitmq.RabbitMQMessage() as held:
            return held.content_type or "", held.body
        case Binding.MQTT5, MqttMessage() as held:
            return held.content_type.default_value(""), held.payload
        case Binding.MQTT311, MqttMessage() as held:
            return "application/cloudevents+json", held.payload
        case Binding.NATS, NatsMessage() as held:
            return next((value for key, value in held.headers.items() if key.lower() == "content-type"), ""), held.payload
        case _:
            raise TypeError(f"{binding.value}:{type(message).__name__}")


def _message_body_size(message: Message, binding: Binding, /) -> int:
    return len(_parts(message, binding)[1])


def _ceiling(value: int, /) -> int:
    if isinstance(value, bool) or value <= 0:
        raise ValueError("payload ceiling must be a positive integer")
    return value


def _header(headers: dict[str, str], name: str, /) -> str:
    return next((value for key, value in headers.items() if key.lower() == name), "")


def _raised(parts: tuple[str, bytes], message: Message, binding: Binding, formats: EventFormat, /) -> RuntimeResult[Decoded]:
    media, body = parts
    if not media:
        return _binary_decoded(message, binding, formats)
    return boundary(
        BINDING_DECODE,
        lambda: parse_media(media),
        catch=(TypeError, ValueError),
    ).bind(
        lambda parsed: formats.decode(body, media=media).bind(formats.admit)
        if _event_media(parsed)
        else _binary_decoded(message, binding, formats)
    )


def _binary_decoded(message: Message, binding: Binding, formats: EventFormat, /) -> RuntimeResult[Decoded]:
    return boundary(
        BINDING_DECODE,
        lambda: _raised_binary(message, binding, formats),
        catch=(CloudEventValidationError, TypeError, ValueError, UnicodeError, OverflowError),
    ).bind(formats.admitted)


def _event_media(media: MediaType, /) -> bool:
    return media.maintype == "application" and (
        media.subtype.startswith("cloudevents+") or media.subtype.startswith("cloudevents-batch+")
    )


def _attribute_name(row: BindingRow, name: str, /) -> str:
    return f"{row.prefix.value}{name}"


def _http_headers(attributes: Iterable[tuple[str, str]], row: BindingRow, /) -> dict[str, str]:
    target = BINDINGS[Binding.HTTP]
    prefix = row.prefix.value
    return {
        _attribute_name(target, name.removeprefix(prefix)): value
        for name, value in attributes
        if not prefix or name.startswith(prefix)
    }


def _byte_header(headers: dict[str, bytes], name: str, /) -> str:
    return next((value.decode() for key, value in headers.items() if key.lower() == name), "")


def _setting(settings: Settings, name: str, binding: Binding, /) -> RuntimeResult[str]:
    return settings.try_find(name).to_result_with(lambda: BINDING_ADMIT.raised(binding.value, f"missing-{name}"))


def _settings(row: BindingRow, settings: Settings, /) -> RuntimeResult[None]:
    unknown = frozenset(settings.keys()) - row.settings
    return (
        Ok(None)
        if not unknown
        else Error(BINDING_ADMIT.raised(row.binding.value, f"unknown-setting:{sorted(unknown)[0]}"))
    )


def _required_setting(settings: Settings, name: str, binding: Binding, /) -> str:
    return settings.try_find(name).to_optional() or (_raise_missing(binding, name))


def _raise_missing(binding: Binding, name: str, /) -> Never:
    raise ValueError(f"{binding.value}:missing-{name}")


```

## [03]-[EMISSION]

- Owner: `Emitter` is an `observe` subscription over `observability/hooks#HOOKS` fired facts and never an emit inside a domain fold — a producer fires its fact once and this owner projects it into an message envelope, so the domain never learns a transport exists and a composition with no emitter bound loses no fact. `Delivery` is what a fan across bound bindings answers, carrying accepted beside matched-duplicate as separate halves.
- Law: the modality is `OBSERVE` and nothing else. `VETO` inverts the announcement law a message envelope exists under, letting a broker refusal reject a domain operation that already happened; a raising observe tap parks on the hook fault window while the emitter's own path stays `Ok`, which is exactly the isolation a fact stream needs.
- Law: a decoded batch yields one identity-correlated outcome per event, while the shared provider handle settles only when the complete frame returns. `Settlement` keeps `accepted`, `duplicate`, and `moot` separate because matched duplication and expiry are not acceptance. Batch position and D20 `sequence` establish no transport order. This connection owner exposes no batch producer until one bounded send can prove custody for every event.
- Law: every long-tail state crosses a declared path rather than a default. Poison messages route to the dead-letter address on the subscription's own slice; a redelivery is distinguished from a duplicate by `(source, id)` alone; D20 `sequence` crosses unchanged and ordering remains the consumer's declared window; a producer `recordedtime` after local arrival drops lag to unmeasured and REFUSES rather than publishing a negative reading; an oversized payload takes the row's `dataref` leg; a broker refusing an attribute name surfaces that name rather than the whole message; an extension name colliding with a native transport header refuses at the lowering; an absent `datacontenttype` under the JSON format defaults to that format's own declared payload type.
- Law: a never-shedding consumer closes by FLUSHING, never by cancelling the in-flight window — the drain stops admitting first, then awaits what is already in flight, so a fact accepted at the boundary is never lost to its own teardown.
- Law: `dataclassification` admits through the closed `Classification` vocabulary and gates before the lowering, so a classification a row cannot honor never reaches an encoder; `source` is the producer claim verified against the trust row before any routing decision reads it.
- Entry: `Emitter.bound(points, scope=...)` registers one subscription over a whole hook roster and returns the detacher, so a producer's entire point table is tapped at one grain and the emitter dies with the composition that bound it. `scope` is the composition whose points it reaches — the SAME `ScopeKey` the producer registered under — because two compositions embedding the runtime in one process partition point custody structurally, and an emitter bound at the default scope reaches none of an embedded composition's roster. `Emitter.project` is the one fact-to-envelope arm, so a new fact family is one projection row and no binding is edited.
- Auto: fan-out across bound bindings inherits the bounds the single delivery already takes — the row's own retry class on every hop, the lane's capacity on every thread arm — so it buys concurrency and never a second bound. Refused bindings shed no sibling's delivery, so the caller re-drives exactly what failed.
- Output: `Delivery` carries the two settlement halves, the per-binding refusals, and each binding's `Externalized` — reference, retention, dual/reference projection, and stored byte count — when payload data externalized; outcome SEMANTICS stay the producing surface's, on `transport/roots#STORE`'s own split.
- Growth: a new fact family is one `project` row; a new bound binding is one member of the emitter's set with no projection edited; a new long-tail state is one declared path on this cluster and one arm on the fold that reads it; a new composition is one `ScopeKey` threaded through `bound`, never a sibling emitter.
- Boundary: hook-fact projection and delivery fan only. Mints no hook point, no outcome semantics, no retention window, and no client connection. Rejected: an emit inside a domain fold; a `VETO` subscription over a fact stream; accepted and matched-duplicate folded into one count; batch position treated as event order; a drain that cancels the in-flight window.

```python
# --- [IMPORTS] --------------------------------------------------------------------------
from collections.abc import Awaitable, Callable
from typing import Self, TypeIs

from expression import Error, Option
from expression.collections import Block
from msgspec import Struct

from rasm.runtime.event import MessageEnvelope, Uniqueness
from rasm.runtime.faults import BINDING_ADMIT, BoundaryFault, RuntimeResult
from rasm.runtime.hooks import Attachment, HookId, HookPoint, Hooks
from rasm.runtime.observe import DEFAULT_SCOPE, ScopeKey

# --- [TYPES] ----------------------------------------------------------------------------

type Project[P: Struct] = Callable[[P], RuntimeResult[MessageEnvelope]]
type ProjectionApply = Callable[[HookId, Struct], RuntimeResult[MessageEnvelope]]
type Fan = Callable[[HookId, RuntimeResult[MessageEnvelope], frozenset[Binding]], Awaitable["Delivery"]]

# --- [MODELS] ---------------------------------------------------------------------------


class Delivery(Struct, frozen=True, gc=False):
    accepted: Block[Uniqueness]
    duplicate: Block[Uniqueness]
    refused: Block[tuple[Binding, BoundaryFault]]
    externalized: Block[tuple[Binding, Externalized]]


class Projection(Struct, frozen=True, gc=False):
    point_id: HookId
    payload: type[Struct]
    apply: ProjectionApply

    @classmethod
    def of[P: Struct](cls, point: HookPoint[P], project: Project[P], /) -> Self:
        def apply(point_id: HookId, payload: Struct, /) -> RuntimeResult[MessageEnvelope]:
            if point_id is not point.id or not _payload(payload, point.payload):
                return Error(BINDING_ADMIT.raised("emitter", f"projection-type:{point_id.value}"))
            return project(payload)

        return cls(point_id=point.id, payload=point.payload, apply=apply)


class Emitter(Struct, frozen=True, gc=False):
    projections: Block[Projection]
    bindings: frozenset[Binding]
    fan: Fan

    def bound(self, points: Block[HookPoint[Struct]], /, *, scope: ScopeKey = DEFAULT_SCOPE) -> RuntimeResult[Callable[[], None]]:
        claimed = tuple(row.point_id for row in self.projections)
        if len(claimed) != len(frozenset(claimed)):
            return Error(BINDING_ADMIT.raised("emitter", "duplicate-projection"))
        missing = points.filter(
            lambda point: self.projections.forall(
                lambda row: row.point_id is not point.id or row.payload is not point.payload
            )
        )
        if not missing.is_empty():
            return Error(BINDING_ADMIT.raised("emitter", f"missing-projection:{missing.head().id.value}"))
        return Hooks.subscribe(points, self._observed, scope=scope).map(_detacher)

    def project[P: Struct](self, point_id: HookId, payload: P, /) -> RuntimeResult[MessageEnvelope]:
        return self.projections.filter(lambda row: row.point_id is point_id).try_head().to_result_with(
            lambda: BINDING_ADMIT.raised("emitter", f"missing-projection:{point_id.value}")
        ).bind(lambda row: row.apply(point_id, payload))

    async def _observed[P: Struct](self, point_id: HookId, payload: P, /) -> Delivery:
        return await self.fan(point_id, self.project(point_id, payload), self.bindings)


def _detacher(attachments: Block[Attachment], /) -> Callable[[], None]:
    def detach() -> None:
        for attachment in attachments:
            attachment.close()

    return detach


def _payload[P: Struct](value: Struct, expected: type[P], /) -> TypeIs[P]:
    return isinstance(value, expected)
```

## [04]-[ADAPTER]

- Owner: `BrokerLane` is the ONE connection owner for every dialable protocol and reads the same `BINDINGS` row as lowering — membership shape, settlement join, producer guarantee, poll cadence, lane and portal need, in-flight window, dead-letter route, and drain law all sit beside the carrier facts. Protocol-specific machinery reaches it as one composition-bound `Client` of normalized awaitable thunks, so a seventh protocol is one row beside one bound port: no adapter subclass, second table, or protocol switch inside the lane. `Consumption` is the caller's subscription coordinate and `LaneDrained` the terminal evidence.
- Cases: `Pump` closes the loop vocabulary and each value is a protocol FACT the row states rather than a knob. `POLL` is the blocking librdkafka `poll`/`consume` on a `CapacityLimiter`-bounded `to_thread` lane, sound because every blocking C call releases the GIL. `WORKER` is the single-threaded `pika` connection whose one worker calls `process_data_events` on its own cadence and whose only inbound door is `add_callback_threadsafe`. `READY` is `paho`'s socket-first triple — `socket()` registers on the caller's own readiness and `loop_read`/`loop_write`/`loop_misc` run as bounded steps inside the task group, since `loop_start`'s daemon thread outlives every cancel scope. `NATIVE` is the already-async `nats` client. `REQUEST` is HTTP, where the request IS the crossing and no loop exists to bound.
- Cases: `Grouping` closes membership. `GROUP` is Kafka's cooperative-sticky consumer group: `on_assign` calls `incremental_assign` and `on_revoke`/`on_lost` call `incremental_unassign`, so a rebalance moves exactly the partitions that changed hands and every other member keeps fetching. `QUEUE` is one message to one member of a named set — a NATS queue group, an MQTT 5.0 shared subscription. `WORK` is RabbitMQ's competing consumers over one queue under `basic_qos` prefetch. `NONE` is a fan where every consumer sees every message, which is what makes a non-shared MQTT topic and a core NATS subject unfit for a work split.
- Law: NO lane creates or owns a loop, and `lifecycle` defaults `caller-owned`. `bound(group, ...)` composes every leg inside the caller's `anyio` task group, so the poll loop's lifetime IS that group's and a cancelled scope reaches a checkpoint rather than orphaning a thread. Sync clients ride the row's own `CapacityLimiter`, and every callback the client fires on its own thread re-enters through ONE `BlockingPortalProvider` — the portal is per lane and never per callback, since a provider minted inside a callback is a second loop owner in the shape the ban exists to foreclose. `Pump.NATIVE` composes under the anyio asyncio backend and forfeits trio on its own `degrade`, stated rather than assumed.
- Law: a settlement never outruns the durable write it stands for. `Settle.JOURNAL` rows disarm every automatic path at construction — Kafka takes `enable.auto.commit=false` beside `enable.auto.offset.store=false`, RabbitMQ takes `auto_ack=False`, JetStream takes an explicit acknowledgement policy — and the lane stores the offset only after `observability/journal#FACT` reports the write durable, then commits synchronously and reads the per-partition `.error` off the answered `TopicPartition` list. Committed offsets sit ONE PAST the message offset, so a lane storing the delivered offset replays the last message on every restart. Automatic commit is the deleted form outright: it acknowledges what a crash then loses, and that loss is invisible at both ends.
- Law: the producer guarantee is the row's, and a `deliver` claim better than at-least-once BUYS a boundary rather than asserting one. `Producing.IDEMPOTENT` arms the producer's own sequencing so a broker-side retry mints no duplicate. `TRANSACTIONAL` brackets one explicit delivered block with begin, the awaitable unit, durable record plus offset send, and commit; every failing leg aborts before its typed fault returns for the standing broker retry policy. `CONFIRMED` is publisher confirms, where a publish either round-trips or the composition declared at-most-once; `UNCONFIRMED` states that fire-and-forget in the open.
- Law: rebalance callbacks fire on whichever thread drove the poll, so they cross the portal and start NO work of their own. Each callback records the assignment delta as a value and returns; the task group reads it and reacts. Starting a fetch, a commit, or a journal write from inside a rebalance callback runs library work on the client's own thread under a lock the client holds, which is the deadlock the portal boundary exists to foreclose, and a callback that raises kills the network loop it fired on rather than surfacing.
- Law: backpressure is ONE bound, not two. `CapacityLimiter` bounds concurrent handler work and the row's `prefetch` bounds what the broker hands this member before it stops feeding — Kafka's fetch window, RabbitMQ's `basic_qos` count, MQTT's in-flight maximum, JetStream's batch — and the two are sized together so a saturated handler stops the broker rather than growing an unbounded in-memory queue behind it. Raising prefetch above the limiter buys latency the lane then pays as memory.
- Law: delivery custody is one serialized state agent, not a shared mutable cell. Provider coordinates key frame ownership while `(source, id)` alone keys deduplication and journal verdicts. Commands cross one memory-stream door and each has one reply stream; the sole consumer replaces immutable state after each transition. Publish and receive steps hold an activity lease, so drain closes admission before waiting for both `active == 0` and empty `inflight`; the caller's deadline scope bounds that event-driven reply without polling.
- Law: a poison message with a bound dead-letter address routes there and settles its original handle only after that publish confirms; only then does it enter `Drained.shed`. Without that address the decode refusal and shed evidence return while the handle remains live, so the operator sees an unconfigured poison route rather than a fabricated settlement. Redelivery is distinguished from duplication by `(source, id)` alone.
- Law: the drain FLUSHES and never cancels. `drained(deadline)` closes admission through the state agent, awaits its empty-window reply under one deadline scope, then flushes the producer — `flush` polls until the queue empties, so every pending delivery report lands and `purge` surfaces an unsent message as its own report rather than dropping it silently — and only then closes. Cancelling the in-flight window instead loses facts already accepted at the boundary, which is exactly the loss the acceptance promised against.
- Entry: `BrokerLane.bound(group, binding, client, consumption, context, settings, formats, trust, store)` is the composition entry. Each protocol binder's authenticated receive arm places a `PrincipalScope` beside the provider message; the generic lane receives no credentials and supplies no fallback scope. `Client.payload_limit(connection)` exposes the safe live event-body ceiling after the protocol binder reserves frame/header overhead; `published` lowers once to measure the selected mode, externalizes past the effective threshold, re-lowers the projected envelope, and answers `Published` with its `Externalized`. `consumed` is the result-typed async iterator every handler drains. `drained(deadline)` is terminal, and `transacted(delivered, unit)` brackets only the explicit delivered block whose handles the package must send into its transaction.
- Auto: ingress ADMITS through `execution/admission#TENANCY` and inherits nothing. The generated event profile intentionally carries no tenant claim; `TenantAdoption` compares the composition-authenticated principal scope beside the provider delivery against the admitted deployment axis and source issuer row. `source` remains an untrusted event claim verified before routing, and missing or unknown `dataclassification` refuses before trust, routing, or settlement. The admitted delivery retains `TenantAdoption`, so downstream work consumes the verified principal, tenant, and issuer row rather than re-resolving them.
- Auto: every package thunk returns through one binding fault anchor. Retry schedules, failure windows, and broker throttle directives remain composition-owned resilience policy; the lane neither duplicates those owners nor performs an invisible retry around a transaction.
- Auto: `Delivered.lag` measures local arrival minus producer `recordedtime` once. An expired `expirytime` yields `Moot` immediately, remains attached to the provider delivery so a shared frame settles whole, bypasses the durable fact record, and lands under the distinct `MOOT` verdict. `LaneDrained` keeps acceptance, duplicate, moot, and shed evidence disjoint.
- Output: `Published` carries the event composite beside the exact `Externalized` store when externalization occurred. `Settlement` and `LaneDrained` carry accepted, matched-duplicate, and moot composites separately; `LaneDrained` additionally carries the finished window and shed causes, so clean, expired, duplicate, and lossy crossings never collapse.
- Growth: a new protocol is one `BINDINGS` row beside one bound `Client`; a new membership shape is one `Grouping` member with its arm on the one membership fold; a new settlement join is one `Settle` member; a new producer guarantee is one `Producing` member with its bracket; a new loop cadence is one `Pump` member with its step; a new dead-letter route is one value on the subscription's slice.
- Boundary: connection lifetime, membership, settlement, and drain only. Mints no message envelope, no format, no retry curve, no failure window, no outcome semantics, and no hook point. Rejected: a lane creating a loop or a thread the caller's group does not own; `loop_start`'s daemon thread; a `BlockingPortalProvider` minted per callback; an automatic offset commit; a prefetch unpaired with its limiter; work started inside a rebalance callback; a drain that cancels the in-flight window; a per-protocol adapter class beside the one row-driven lane.

```python
# --- [IMPORTS] --------------------------------------------------------------------------
from collections.abc import AsyncIterator, Awaitable, Callable
from datetime import UTC, datetime
from enum import StrEnum
from typing import Annotated, Final, Self

import anyio
from anyio import BrokenResourceError, CancelScope, CapacityLimiter, ClosedResourceError, create_memory_object_stream
from anyio.abc import TaskGroup
from anyio.from_thread import BlockingPortalProvider
from anyio.streams.memory import MemoryObjectReceiveStream, MemoryObjectSendStream
from expression import Error, Nothing, Ok, Option, Result, Some
from expression.collections import Block, Map
from msgspec import Meta, Struct

from rasm.runtime.admission import Claim, Classification, PrincipalScope, RuntimeContext, TenantAdoption, Trust
from rasm.runtime.event import MessageEnvelope, Uniqueness, stamped
from rasm.runtime.faults import (
    BINDING_ADMIT,
    BINDING_CONNECT,
    BINDING_DECODE,
    BINDING_DRAIN,
    BINDING_ENCODE,
    BINDING_SETTLE,
    BINDING_TRANSACTION,
    BoundaryFault,
    Catch,
    RuntimeResult,
    async_boundary,
    boundary,
)


# --- [TYPES] ----------------------------------------------------------------------------

type Dial = Callable[
    [TaskGroup, "Consumption", Settings, Option[CapacityLimiter], Option[BlockingPortalProvider]],
    Awaitable[object],
]
type Emit = Callable[[object, Message], Awaitable[None]]
type Step = Callable[[object, float], Awaitable[Block["Received"]]]
type Empty = Callable[[object, "Received"], Awaitable[None]]
type Record = Callable[[Block["Delivered"]], Awaitable[Map[Uniqueness, "SettlementVerdict"]]]
type SettleThunk = Callable[[object, Block["Delivered"]], Awaitable[None]]
type DeadLetter = Callable[[object, str, "Received", BoundaryFault], Awaitable[None]]
type TransactionThunk = Callable[[object], Awaitable[None]]
type Flush = Callable[[object, float], Awaitable[int]]
type Shut = Callable[[object], Awaitable[None]]
type PayloadLimit = Callable[[object], int]


# --- [MODELS] ---------------------------------------------------------------------------


class Client(Struct, frozen=True, gc=False):
    dial: Dial
    emit: Emit
    step: Step
    empty: Empty
    record: Record
    settle: SettleThunk
    dead_letter: DeadLetter
    begin: TransactionThunk
    commit: TransactionThunk
    abort: TransactionThunk
    flush: Flush
    shut: Shut
    payload_limit: PayloadLimit
    raises: Catch


class Consumption(Struct, frozen=True, gc=False):
    addresses: Block[str]
    member: Option[str] = Nothing
    dead_letter: Option[str] = Nothing


class Received(Struct, frozen=True, gc=False):
    message: Message
    authority: PrincipalScope
    coordinate: "ProviderCoordinate"
    handle: object


class ProviderCoordinate(Struct, frozen=True, order=True, gc=False):
    value: Annotated[str, Meta(min_length=1)]


class SettlementVerdict(StrEnum):
    ACCEPTED = "accepted"
    DUPLICATE = "duplicate"
    MOOT = "moot"


class Delivered(Struct, frozen=True, gc=False):
    envelope: MessageEnvelope
    adoption: TenantAdoption
    coordinate: ProviderCoordinate
    composite: Uniqueness
    handle: object
    lag: Option[float]
    expiry: Option[datetime]
    moot: bool


class Moot(Struct, frozen=True, gc=False):
    delivery: Delivered
    expiry: datetime


type DeliveryOutcome = Delivered | Moot


class Published(Struct, frozen=True, gc=False):
    composite: Uniqueness
    externalized: Option[Externalized]


class Prepared(Struct, frozen=True, gc=False):
    envelope: MessageEnvelope
    message: Message
    externalized: Option[Externalized]


class LaneState(Struct, frozen=True, gc=False):
    admitting: bool = True
    active: int = 0
    inflight: Map[Uniqueness, Delivered] = Map.empty()
    frames: Map[ProviderCoordinate, frozenset[Uniqueness]] = Map.empty()
    claimed: frozenset[Uniqueness] = frozenset()
    prepared: Map[Uniqueness, SettlementVerdict] = Map.empty()
    settled: Map[Uniqueness, SettlementVerdict] = Map.empty()
    shed: Block[tuple[Option[Uniqueness], BoundaryFault]] = Block.empty()


class LaneSnapshot(Struct, frozen=True, gc=False):
    accepted: Block[Uniqueness]
    duplicate: Block[Uniqueness]
    moot: Block[Uniqueness]
    finished: int
    shed: Block[tuple[Option[Uniqueness], BoundaryFault]]


class LaneDrained(Struct, frozen=True, gc=False):
    flushed: int
    accepted: Block[Uniqueness]
    duplicate: Block[Uniqueness]
    moot: Block[Uniqueness]
    finished: int
    shed: Block[tuple[Option[Uniqueness], BoundaryFault]]


class Settlement(Struct, frozen=True, gc=False):
    accepted: Block[Uniqueness]
    duplicate: Block[Uniqueness]
    moot: Block[Uniqueness]


class EnterCommand(Struct, tag="enter", frozen=True, gc=False):
    reply: MemoryObjectSendStream[bool]


class LeaveCommand(Struct, tag="leave", frozen=True, gc=False):
    reply: MemoryObjectSendStream[None]


class TrackCommand(Struct, tag="track", frozen=True, gc=False):
    delivered: Block[Delivered]
    reply: MemoryObjectSendStream[RuntimeResult[None]]


class LiveCommand(Struct, tag="live", frozen=True, gc=False):
    delivered: Block[Delivered]
    reply: MemoryObjectSendStream[RuntimeResult[None]]


class ClaimCommand(Struct, tag="claim", frozen=True, gc=False):
    delivered: Block[Delivered]
    reply: MemoryObjectSendStream[RuntimeResult[None]]


class ReleaseCommand(Struct, tag="release", frozen=True, gc=False):
    delivered: Block[Delivered]
    reply: MemoryObjectSendStream[None]


class PreparedCommand(Struct, tag="prepared", frozen=True, gc=False):
    identities: frozenset[Uniqueness]
    reply: MemoryObjectSendStream[Map[Uniqueness, SettlementVerdict]]


class RetainCommand(Struct, tag="retain", frozen=True, gc=False):
    verdicts: Map[Uniqueness, SettlementVerdict]
    reply: MemoryObjectSendStream[None]


class LandCommand(Struct, tag="land", frozen=True, gc=False):
    delivered: Block[Delivered]
    verdicts: Map[Uniqueness, SettlementVerdict]
    reply: MemoryObjectSendStream[Settlement]


class DrainCommand(Struct, tag="drain", frozen=True, gc=False):
    reply: MemoryObjectSendStream[LaneSnapshot]


class ShedCommand(Struct, tag="shed", frozen=True, gc=False):
    key: Option[Uniqueness]
    fault: BoundaryFault
    reply: MemoryObjectSendStream[None]


type LaneCommand = (
    EnterCommand
    | LeaveCommand
    | TrackCommand
    | LiveCommand
    | ClaimCommand
    | ReleaseCommand
    | PreparedCommand
    | RetainCommand
    | LandCommand
    | DrainCommand
    | ShedCommand
)


class LaneAgent(Struct, frozen=True, gc=False):
    door: MemoryObjectSendStream[LaneCommand]

    async def enter(self, /) -> bool:
        return await _answer(self.door, lambda reply: EnterCommand(reply))

    async def leave(self, /) -> None:
        await _answer(self.door, lambda reply: LeaveCommand(reply))

    async def track(self, delivered: Block[Delivered], /) -> RuntimeResult[None]:
        return await _answer(self.door, lambda reply: TrackCommand(delivered, reply))

    async def live(self, delivered: Block[Delivered], /) -> RuntimeResult[None]:
        return await _answer(self.door, lambda reply: LiveCommand(delivered, reply))

    async def claim(self, delivered: Block[Delivered], /) -> RuntimeResult[None]:
        return await _answer(self.door, lambda reply: ClaimCommand(delivered, reply))

    async def release(self, delivered: Block[Delivered], /) -> None:
        await _answer(self.door, lambda reply: ReleaseCommand(delivered, reply))

    async def prepared(self, identities: frozenset[Uniqueness], /) -> Map[Uniqueness, SettlementVerdict]:
        return await _answer(self.door, lambda reply: PreparedCommand(identities, reply))

    async def retain(self, verdicts: Map[Uniqueness, SettlementVerdict], /) -> None:
        await _answer(self.door, lambda reply: RetainCommand(verdicts, reply))

    async def land(
        self, delivered: Block[Delivered], verdicts: Map[Uniqueness, SettlementVerdict], /
    ) -> Settlement:
        return await _answer(self.door, lambda reply: LandCommand(delivered, verdicts, reply))

    async def drain(self, /) -> MemoryObjectReceiveStream[LaneSnapshot]:
        reply, answer = create_memory_object_stream[LaneSnapshot](1)
        await self.door.send(DrainCommand(reply))
        return answer

    async def shed(self, key: Option[Uniqueness], fault: BoundaryFault, /) -> None:
        await _answer(self.door, lambda reply: ShedCommand(key, fault, reply))


async def _answer[T](
    door: MemoryObjectSendStream[LaneCommand], command: Callable[[MemoryObjectSendStream[T]], LaneCommand], /
) -> T:
    reply, answer = create_memory_object_stream[T](1)
    async with reply, answer:
        await door.send(command(reply))
        return await answer.receive()


async def _respond[T](reply: MemoryObjectSendStream[T], value: T, /) -> None:
    try:
        await reply.send(value)
    except (BrokenResourceError, ClosedResourceError):
        pass
    finally:
        await reply.aclose()


async def _lane_state(
    binding: Binding, commands: MemoryObjectReceiveStream[LaneCommand], /
) -> None:
    state = LaneState()
    draining: Block[MemoryObjectSendStream[LaneSnapshot]] = Block.empty()
    async with commands:
        async for command in commands:
            match command:
                case EnterCommand(reply=reply):
                    entered = state.admitting
                    if entered:
                        state = replace(state, active=state.active + 1)
                    await _respond(reply, entered)
                case LeaveCommand(reply=reply):
                    state = replace(state, active=state.active - 1)
                    await _respond(reply, None)
                case TrackCommand(delivered=delivered, reply=reply):
                    state, outcome = _tracked_state(state, delivered, binding)
                    await _respond(reply, outcome)
                case LiveCommand(delivered=delivered, reply=reply):
                    await _respond(reply, _live_state(state, delivered, binding))
                case ClaimCommand(delivered=delivered, reply=reply):
                    state, outcome = _claimed_state(state, delivered, binding)
                    await _respond(reply, outcome)
                case ReleaseCommand(delivered=delivered, reply=reply):
                    state = replace(
                        state,
                        claimed=state.claimed - frozenset(held.composite for held in delivered),
                    )
                    await _respond(reply, None)
                case PreparedCommand(identities=identities, reply=reply):
                    await _respond(
                        reply,
                        Map.of_seq(
                            (identity, state.prepared[identity])
                            for identity in identities
                            if state.prepared.contains_key(identity)
                        )
                    )
                case RetainCommand(verdicts=verdicts, reply=reply):
                    prepared = state.prepared
                    for identity, verdict in verdicts.items():
                        prepared = prepared.add(identity, verdict)
                    state = replace(state, prepared=prepared)
                    await _respond(reply, None)
                case LandCommand(delivered=delivered, verdicts=verdicts, reply=reply):
                    state, settlement = _landed_state(state, delivered, verdicts)
                    await _respond(reply, settlement)
                case DrainCommand(reply=reply):
                    state = replace(state, admitting=False)
                    draining = draining.append(Block.singleton(reply))
                case ShedCommand(key=key, fault=fault, reply=reply):
                    state = replace(state, shed=state.shed.append(Block.singleton((key, fault))))
                    await _respond(reply, None)
            if state.inflight.is_empty() and state.active == 0 and not draining.is_empty():
                snapshot = _snapshot(state)
                for reply in draining:
                    await _respond(reply, snapshot)
                draining = Block.empty()


def _tracked_state(
    state: LaneState, delivered: Block[Delivered], binding: Binding, /
) -> tuple[LaneState, RuntimeResult[None]]:
    if delivered.is_empty():
        return state, Ok(None)
    coordinates = frozenset(held.coordinate for held in delivered)
    identities = frozenset(held.composite for held in delivered)
    if len(coordinates) != 1 or len(identities) != len(delivered):
        return state, Error(BINDING_DECODE.raised(binding.value, "invalid-delivery-frame"))
    coordinate = next(iter(coordinates))
    if state.frames.contains_key(coordinate) or any(
        state.inflight.contains_key(identity) or state.settled.contains_key(identity)
        for identity in identities
    ):
        return state, Error(BINDING_DECODE.raised(binding.value, "duplicate-delivery-identity"))
    inflight = state.inflight
    for held in delivered:
        inflight = inflight.add(held.composite, held)
    return replace(
        state,
        frames=state.frames.add(coordinate, identities),
        inflight=inflight,
    ), Ok(None)


def _live_state(state: LaneState, delivered: Block[Delivered], binding: Binding, /) -> RuntimeResult[None]:
    identities = frozenset(held.composite for held in delivered)
    if len(identities) != len(delivered):
        return Error(BINDING_SETTLE.raised(binding.value, "repeated-delivery"))
    coordinates = frozenset(held.coordinate for held in delivered)
    if any(
        state.frames.try_find(coordinate).default_value(frozenset())
        != frozenset(held.composite for held in delivered if held.coordinate == coordinate)
        for coordinate in coordinates
    ):
        return Error(BINDING_SETTLE.raised(binding.value, "partial-frame-settlement"))
    for held in delivered:
        standing = state.inflight.try_find(held.composite)
        if standing.is_none() or standing.default_value(held) != held:
            return Error(BINDING_SETTLE.raised(binding.value, "unadmitted-settlement"))
    return Ok(None)


def _claimed_state(
    state: LaneState, delivered: Block[Delivered], binding: Binding, /
) -> tuple[LaneState, RuntimeResult[None]]:
    identities = frozenset(held.composite for held in delivered)
    if not identities.isdisjoint(state.claimed):
        return state, Error(BINDING_SETTLE.raised(binding.value, "delivery-already-settling"))
    return replace(state, claimed=state.claimed | identities), Ok(None)


def _landed_state(
    state: LaneState,
    delivered: Block[Delivered],
    verdicts: Map[Uniqueness, SettlementVerdict],
    /,
) -> tuple[LaneState, Settlement]:
    accepted: Block[Uniqueness] = Block.empty()
    duplicate: Block[Uniqueness] = Block.empty()
    moot: Block[Uniqueness] = Block.empty()
    inflight = state.inflight
    prepared = state.prepared
    settled = state.settled
    frames = state.frames
    for held in delivered:
        verdict = verdicts[held.composite]
        inflight = inflight.remove(held.composite)
        prepared = prepared.remove(held.composite)
        settled = settled.add(held.composite, verdict)
        match verdict:
            case SettlementVerdict.ACCEPTED:
                accepted = accepted.append(Block.singleton(held.composite))
            case SettlementVerdict.DUPLICATE:
                duplicate = duplicate.append(Block.singleton(held.composite))
            case SettlementVerdict.MOOT:
                moot = moot.append(Block.singleton(held.composite))
    for coordinate in frozenset(held.coordinate for held in delivered):
        frames = frames.remove(coordinate)
    return replace(
        state,
        inflight=inflight,
        frames=frames,
        claimed=state.claimed - frozenset(held.composite for held in delivered),
        prepared=prepared,
        settled=settled,
    ), Settlement(accepted=accepted, duplicate=duplicate, moot=moot)


def _snapshot(state: LaneState, /) -> LaneSnapshot:
    return LaneSnapshot(
        accepted=_settled_block(state, SettlementVerdict.ACCEPTED),
        duplicate=_settled_block(state, SettlementVerdict.DUPLICATE),
        moot=_settled_block(state, SettlementVerdict.MOOT),
        finished=len(state.settled),
        shed=state.shed,
    )


def _settled_block(state: LaneState, verdict: SettlementVerdict, /) -> Block[Uniqueness]:
    return Block.of_seq(identity for identity, held in state.settled.items() if held is verdict)


# --- [SERVICES] -------------------------------------------------------------------------


class BrokerLane(Struct, frozen=True, gc=False):
    row: BindingRow
    client: Client
    consumption: Consumption
    context: RuntimeContext
    connection: object
    settings: Settings
    formats: EventFormat
    trust: Trust
    store: Option[PayloadStore]
    agent: LaneAgent
    limiter: Option[CapacityLimiter]
    portal: Option[BlockingPortalProvider]

    @classmethod
    async def bound(
        cls,
        group: TaskGroup,
        binding: Binding,
        client: Client,
        consumption: Consumption,
        context: RuntimeContext,
        settings: Settings,
        formats: EventFormat,
        trust: Trust,
        store: Option[PayloadStore],
        /,
    ) -> RuntimeResult[Self]:
        match BINDINGS.try_find(binding):
            case Option(tag="none"):
                return Error(BINDING_ADMIT.raised(binding.value, "provider-unavailable"))
            case Option(tag="some", some=row) if row.arm is Arm.ABSENT:
                return Error(BINDING_ADMIT.raised(binding.value, "provider-unavailable"))
            case Option(tag="some", some=row):
                limiter = Some(CapacityLimiter(row.prefetch)) if row.lane.is_some() else Nothing
                portal = Some(BlockingPortalProvider()) if row.portal else Nothing
        admitted_settings = _settings(row, settings)
        if admitted_settings.is_error():
            return admitted_settings
        connected = await async_boundary(
            BINDING_CONNECT,
            lambda: client.dial(group, consumption, settings, limiter, portal),
            catch=client.raises,
        )
        match connected:
            case Result(tag="error") as refused:
                return refused
            case Result(tag="ok", ok=connection):
                door, commands = create_memory_object_stream[LaneCommand](max(1, row.prefetch))
                group.start_soon(_lane_state, binding, commands)
                return Ok(
                    cls(
                        row=row,
                        client=client,
                        consumption=consumption,
                        context=context,
                        connection=connection,
                        settings=settings,
                        formats=formats,
                        trust=trust,
                        store=store,
                        agent=LaneAgent(door),
                        limiter=limiter,
                        portal=portal,
                    )
                )

    async def published(
        self, envelope: MessageEnvelope, /, *, mode: Content, suffix: Option[Suffix] = Nothing
    ) -> RuntimeResult[Published]:
        if not await self.agent.enter():
            return Error(BINDING_DRAIN.raised(self.row.binding.value, "lane-closed"))
        try:
            match await self._publish_prepared(envelope, mode, suffix):
                case Result(tag="error") as refused:
                    return refused
                case Result(tag="ok", ok=prepared):
                    return (await async_boundary(
                        BINDING_ENCODE,
                        lambda: self.client.emit(self.connection, prepared.message),
                        catch=self.client.raises,
                    )).map(
                        lambda _: Published(
                            composite=Uniqueness.of(prepared.envelope),
                            externalized=prepared.externalized,
                        )
                    )
        finally:
            with CancelScope(shield=True):
                await self.agent.leave()

    async def _publish_prepared(
        self, envelope: MessageEnvelope, mode: Content, suffix: Option[Suffix], /
    ) -> RuntimeResult[Prepared]:
        lowered = lower(
            envelope,
            binding=self.row.binding,
            mode=mode,
            suffix=suffix,
            settings=self.settings,
            formats=self.formats,
        )
        match lowered, boundary(
            BINDING_ADMIT,
            lambda: _ceiling(self.client.payload_limit(self.connection)),
            catch=(TypeError, ValueError, OverflowError),
        ):
            case Result(tag="error") as refused, _:
                return refused
            case _, Result(tag="error") as refused:
                return refused
            case Result(tag="ok", ok=message), Result(tag="ok", ok=ceiling):
                size = _message_body_size(message, self.row.binding)
        threshold = ceiling if self.row.dataref.negotiated else min(ceiling, self.row.dataref.threshold)
        if size <= threshold:
            return Ok(Prepared(envelope=envelope, message=message, externalized=Nothing))
        projection = self.row.dataref.projection
        if projection is CarriageMode.DUAL and size > ceiling:
            if self.settings.try_find("datarefprojection") != Some(CarriageMode.REFERENCE.value):
                return Error(BINDING_ADMIT.raised(self.row.binding.value, "dual-over-ceiling-without-negotiation"))
            projection = CarriageMode.REFERENCE
        match self.store:
            case Option(tag="none"):
                return Error(BINDING_ADMIT.raised(self.row.binding.value, "dataref-without-store"))
            case Option(tag="some", some=store):
                externalized = await store.externalize(envelope, self.row.dataref, projection)
        match externalized:
            case Result(tag="error") as refused:
                return refused
            case Result(tag="ok", ok=(projected, stored)):
                reframed = lower(
                    projected,
                    binding=self.row.binding,
                    mode=mode,
                    suffix=suffix,
                    settings=self.settings,
                    formats=self.formats,
                )
        return reframed.bind(
            lambda framed: Ok(
                Prepared(envelope=projected, message=framed, externalized=Some(stored))
            )
            if _message_body_size(framed, self.row.binding) <= ceiling
            else Error(BINDING_ENCODE.raised(self.row.binding.value, "dataref-frame-over-ceiling"))
        )

    async def consumed(self) -> AsyncIterator[RuntimeResult[DeliveryOutcome]]:
        while await self.agent.enter():
            try:
                stepped = await async_boundary(
                    BINDING_DECODE,
                    lambda: self.client.step(self.connection, 1.0),
                    catch=self.client.raises,
                )
                match stepped:
                    case Result(tag="error") as refused:
                        yield refused
                        return
                    case Result(tag="ok", ok=received):
                        for held in received:
                            raised = raise_(held.message, binding=self.row.binding, formats=self.formats)
                            match raised:
                                case Result(tag="error") as refused:
                                    rerouted = await self._shed(held, Nothing, refused.error)
                                    yield refused if rerouted.is_ok() else rerouted
                                case Result(tag="ok", ok=decoded):
                                    if decoded.events.is_empty():
                                        emptied = await async_boundary(
                                            BINDING_SETTLE,
                                            lambda: self.client.empty(self.connection, held),
                                            catch=self.client.raises,
                                        )
                                        if emptied.is_error():
                                            yield emptied
                                        continue
                                    live: Block[Delivered] = Block.empty()
                                    refused: Block[RuntimeResult[Delivered]] = Block.empty()
                                    faults: Block[BoundaryFault] = Block.empty()
                                    for envelope in decoded.events:
                                        delivered = self._delivery(
                                            envelope, held.authority, held.coordinate, held.handle
                                        )
                                        match delivered:
                                            case Result(tag="ok", ok=admitted):
                                                live = live.append(Block.singleton(admitted))
                                            case Result(tag="error") as rejected:
                                                refused = refused.append(Block.singleton(rejected))
                                                faults = faults.append(Block.singleton(rejected.error))
                                    if not refused.is_empty():
                                        rerouted = await self._shed(held, Nothing, faults.head())
                                        if rerouted.is_error():
                                            yield rerouted
                                        for rejected in refused:
                                            yield rejected
                                        continue
                                    tracked = await self.agent.track(live)
                                    match tracked:
                                        case Result(tag="error") as rejected:
                                            rerouted = await self._shed(held, Nothing, rejected.error)
                                            yield rejected if rerouted.is_ok() else rerouted
                                        case Result(tag="ok"):
                                            for admitted in live:
                                                match admitted.expiry:
                                                    case Option(tag="some", some=expiry) if admitted.moot:
                                                        yield Ok(Moot(delivery=admitted, expiry=expiry))
                                                    case _:
                                                        yield Ok(admitted)
            finally:
                with CancelScope(shield=True):
                    await self.agent.leave()
            await anyio.lowlevel.checkpoint()

    async def settled(self, delivered: Block[Delivered], /) -> RuntimeResult[Settlement]:
        if delivered.is_empty():
            return Ok(Settlement(accepted=Block.empty(), duplicate=Block.empty(), moot=Block.empty()))
        live = await self.agent.live(delivered)
        if live.is_error():
            return live
        claimed = await self.agent.claim(delivered)
        if claimed.is_error():
            return claimed
        settled = await self._settlement_prepared(delivered)
        match settled:
            case Result(tag="error") as refused:
                await self.agent.release(delivered)
                return refused
            case Result(tag="ok", ok=verdicts):
                return Ok(await self.agent.land(delivered, verdicts))

    async def _settlement_prepared(
        self, delivered: Block[Delivered], /
    ) -> RuntimeResult[Map[Uniqueness, SettlementVerdict]]:
        identities = frozenset(held.composite for held in delivered)
        retained = await self.agent.prepared(identities)
        if not retained.is_empty() and len(retained) != len(identities):
            return Error(BINDING_SETTLE.raised(self.row.binding.value, "partial-record-verdict"))
        active = delivered.filter(lambda held: not held.moot)
        if not retained.is_empty():
            recorded: RuntimeResult[Map[Uniqueness, SettlementVerdict]] = Ok(retained)
        elif active.is_empty():
            recorded = Ok(Map.empty())
        elif self.row.settlement is Settle.JOURNAL:
            recorded = await async_boundary(
                BINDING_SETTLE,
                lambda: self.client.record(active),
                catch=self.client.raises,
            )
        else:
            recorded = Ok(Map.of_seq((held.composite, SettlementVerdict.ACCEPTED) for held in active))
        match recorded:
            case Result(tag="error") as refused:
                return refused
            case Result(tag="ok", ok=verdicts):
                for held in delivered.filter(lambda item: item.moot):
                    verdicts = verdicts.add(held.composite, SettlementVerdict.MOOT)
                if frozenset(verdicts.keys()) != identities:
                    return Error(BINDING_SETTLE.raised(self.row.binding.value, "record-verdict-mismatch"))
                await self.agent.retain(verdicts)
                settled = await async_boundary(
                    BINDING_SETTLE,
                    lambda: self.client.settle(self.connection, delivered),
                    catch=self.client.raises,
                )
                return settled.map(lambda _: verdicts)

    async def transacted[T](
        self, delivered: Block[Delivered], unit: Callable[[Block[Delivered]], Awaitable[RuntimeResult[T]]], /
    ) -> RuntimeResult[T]:
        if self.row.producing is not Producing.TRANSACTIONAL:
            return Error(BINDING_TRANSACTION.raised(self.row.binding.value, self.row.producing.value))
        if delivered.is_empty():
            return Error(BINDING_TRANSACTION.raised(self.row.binding.value, "empty-delivery-unit"))
        live = await self.agent.live(delivered)
        if live.is_error():
            return live
        claimed = await self.agent.claim(delivered)
        if claimed.is_error():
            return claimed
        opened = await async_boundary(
            BINDING_TRANSACTION,
            lambda: self.client.begin(self.connection),
            catch=self.client.raises,
        )
        if opened.is_error():
            await self.agent.release(delivered)
            return opened
        outcome = await unit(delivered)
        match outcome:
            case Result(tag="error") as refused:
                aborted = await async_boundary(
                    BINDING_TRANSACTION,
                    lambda: self.client.abort(self.connection),
                    catch=self.client.raises,
                )
                await self.agent.release(delivered)
                return refused if aborted.is_ok() else aborted
            case Result(tag="ok", ok=value):
                settled = await self._settlement_prepared(delivered)
                match settled:
                    case Result(tag="error") as refused:
                        aborted = await async_boundary(
                            BINDING_TRANSACTION,
                            lambda: self.client.abort(self.connection),
                            catch=self.client.raises,
                        )
                        await self.agent.release(delivered)
                        return refused if aborted.is_ok() else aborted
                    case Result(tag="ok", ok=verdicts):
                        committed = await async_boundary(
                            BINDING_TRANSACTION,
                            lambda: self.client.commit(self.connection),
                            catch=self.client.raises,
                        )
                        if committed.is_ok():
                            await self.agent.land(delivered, verdicts)
                            return committed.map(lambda _: value)
                        aborted = await async_boundary(
                            BINDING_TRANSACTION,
                            lambda: self.client.abort(self.connection),
                            catch=self.client.raises,
                        )
                        await self.agent.release(delivered)
                        return committed if aborted.is_ok() else aborted

    async def drained(self, /, *, deadline: float) -> RuntimeResult[LaneDrained]:
        if deadline < 0.0:
            return Error(BINDING_DRAIN.raised(self.row.binding.value, "negative-deadline"))
        until = anyio.current_time() + deadline
        snapshot: LaneSnapshot | None = None
        answer = await self.agent.drain()
        async with answer:
            with anyio.move_on_after(deadline):
                snapshot = await answer.receive()
        if snapshot is None:
            return Error(BINDING_DRAIN.raised(self.row.binding.value, "inflight-deadline"))
        flushed = await async_boundary(
            BINDING_DRAIN,
            lambda: self.client.flush(self.connection, max(0.0, until - anyio.current_time())),
            catch=self.client.raises,
        )
        match flushed:
            case Result(tag="error") as refused:
                return refused
            case Result(tag="ok", ok=count):
                closed = await async_boundary(
                    BINDING_DRAIN,
                    lambda: self.client.shut(self.connection),
                    catch=self.client.raises,
                )
                return closed.map(
                    lambda _: Drained(
                        flushed=count,
                        accepted=snapshot.accepted,
                        duplicate=snapshot.duplicate,
                        moot=snapshot.moot,
                        finished=snapshot.finished,
                        shed=snapshot.shed,
                    )
                )

    def _admitted(
        self, envelope: MessageEnvelope, authority: PrincipalScope, /
    ) -> RuntimeResult[TenantAdoption]:
        return _grade(envelope, self.row.binding).bind(
            lambda admitted_grade: TenantAdoption.of(
                self.context,
                self.trust,
                authority,
                Claim(source=envelope.source.reference, grade=admitted_grade),
            )
        )

    def _delivery(
        self,
        envelope: MessageEnvelope,
        authority: PrincipalScope,
        coordinate: ProviderCoordinate,
        handle: object,
        /,
    ) -> RuntimeResult[Delivered]:
        arrived = datetime.now(UTC)
        lag = (
            stamped(envelope.extensions.recordedtime.to_datetime(), arrived).map(Some)
            if envelope.extensions.has_field("recordedtime")
            else Ok(Nothing)
        )
        expiry = (
            Some(envelope.extensions.expirytime.to_datetime())
            if envelope.extensions.has_field("expirytime")
            else Nothing
        )
        return self._admitted(envelope, authority).bind(
            lambda adoption: lag.map(
                lambda measured: Delivered(
                    envelope=envelope,
                    adoption=adoption,
                    coordinate=coordinate,
                    composite=Uniqueness.of(envelope),
                    handle=handle,
                    lag=measured,
                    expiry=expiry,
                    moot=expiry.map(lambda expires: expires <= arrived).default_value(False),
                )
            )
        )

    async def _shed(
        self,
        received: Received,
        key: Option[Uniqueness],
        fault: BoundaryFault,
        /,
    ) -> RuntimeResult[None]:
        match self.consumption.dead_letter:
            case Option(tag="none"):
                await self.agent.shed(key, fault)
                return Ok(None)
            case Option(tag="some", some=address):
                routed = await async_boundary(
                    BINDING_SETTLE,
                    lambda: self.client.dead_letter(self.connection, address, received, fault),
                    catch=self.client.raises,
                )
                if routed.is_error():
                    return routed
                await self.agent.shed(key, fault)
                return Ok(None)
```

## [05]-[RESEARCH]

(none)

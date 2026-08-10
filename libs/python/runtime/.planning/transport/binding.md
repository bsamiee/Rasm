# [PY_RUNTIME_BINDING]

Every protocol lowering of a message envelope seats here: `BINDINGS` is the one row family carrying content mode, header prefix, routing key, `protocolsettings` slice, filter pushdown, execution arm, and payload-residence policy per protocol, and `Emitter` is the `observe` subscription that turns a fired hook fact into an message envelope and hands it to a bound binding. Rows span HTTP, Kafka, both MQTT protocol versions, AMQP 1.0, NATS, and RabbitMQ; growth is one row and every consumer stands untouched, because binding is DATA and never a type a caller switches on.

Specification law owns each lowering and the SDK accelerates four of them: the four SDK binding modules carry distinct prefix families and MQTT carries none, so the MQTT and NATS rows are branch-owned whole. Composed owners: `transport/event#MESSAGE` the message envelope, `transport/event#FORMAT` the codec, `transport/filter#DIALECT` the subscription and its delivery predicates, `transport/roots#STORE` the `dataref` residence, `reliability/resilience#RESILIENCE` every curve, window, and rate, `observability/hooks#HOOKS` the fired facts, `observability/journal#FACT` the durable write, `execution/admission#CONTEXT` the profile and the tenant.

`BrokerLane` closes the connection half the rows describe: one owner drives every protocol's membership, settlement, transactional boundary, poll cadence, and drain off its `ADAPTERS` row and a composition-bound `Client` port, so a seventh protocol is one row beside one bound port, with no adapter class, arm, or subclass to add. No package on this page creates or owns an event loop — every lane composes inside the caller's `anyio` task group and `lifecycle` defaults `caller-owned`.

## [01]-[INDEX]

- [02]-[BINDING]: binding rows — content modes, the prefix families, routing keys, `protocolsettings`, pushdown verdicts, execution arms, and the per-binding `dataref` policy.
- [03]-[EMISSION]: `Emitter` — the hook subscription, its per-event batch settlement, and the declared rails every long-tail state crosses.
- [04]-[ADAPTER]: `BrokerLane` — the per-protocol execution arm, group membership, the journal-joined settlement, transactional boundaries, poll-loop lifetime, backpressure, dead-letter routing, and the flushing drain.

## [02]-[BINDING]

- Owner: `BindingRow` is the branch's whole protocol vocabulary and `BINDINGS` the table every consumer reads — a lowering, a subscription's `protocolsettings` slice, a filter's pushdown verdict, an execution arm, and a payload threshold all derive from one row, so a new protocol is ONE row and no fold, adapter, or admission gate is edited. `Content` closes the mode vocabulary and `Prefix` the header family, both spelled once rather than as literals at four lowering sites.
- Cases: distinct prefix families span the SDK bindings — `ce-` for HTTP and RabbitMQ, `ce_` for Kafka, `cloudEvents_` for AMQP writing and `cloudEvents:` for AMQP reading — while MQTT carries NO prefix at all, its attributes riding bare User Property names, and NATS carries `ce-` under a branch-owned lowering the distribution does not ship. Reading `ce-` as one estate-wide spelling is the drift this table forecloses.
- Cases: mode support is per row and never a caller flag. HTTP and Kafka carry binary, structured, and batch; `Binding.MQTT5` carries binary and structured because User Properties exist there; `Binding.MQTT311` carries STRUCTURED ONLY and states that restriction on its own `degrade`; AMQP 1.0, NATS, and RabbitMQ carry binary and structured. Modes a row does not hold refuse at admission with the row named, never by silently lowering the other way.
- Law: `protocolsettings` is the subscription's own per-binding slice and REPLACES every hand-rolled per-sink knob — HTTP `headers`/`method`; MQTT `topicname` required beside `qos`/`retain`/`expiry`/`userproperties`; AMQP `address`/`linkname`/`sendersettlementmode`/`linkproperties`; Kafka `topicname`/`partitionkeyextractor`/`clientid`/`acks`; NATS `subject`; RabbitMQ the branch's own `exchange`/`routingkey`/`deliverymode`/`expiration`, since the specification carries no RabbitMQ entry. Knobs outside a row's slice are unspellable.
- Law: pushdown is a row verdict, never a runtime probe. MQTT resolves a topic filter at the BROKER through SUBSCRIBE, NATS through subject wildcards, RabbitMQ through the exchange binding on its routing key, and AMQP through link-source filters under a `copy` or `move` distribution mode; Kafka has no server-side filtering and HTTP no native mechanism, so both filter consumer-side. `transport/filter#DIALECT` owns the dialect half of that join and reads THIS column rather than carrying one of its own — a composite pushes only where every child does, and negation and `sql` never do.
- Law: routing keys are the row's, derived from the roster rather than restated — Kafka takes `partitionkey` onto the record key through the SDK's own `_default_key_mapper`, MQTT and NATS take the topic and subject, RabbitMQ the routing key, and HTTP and AMQP take none. Hand-spelled key extractors beside the helper that owns the roster are the deleted form.
- Law: NO row owns `retry`, and no row carries a column for it either. Transport families foreclose the coordinate and `reliability/resilience#RESILIENCE` holds every schedule, so the answer is uniform across the whole family and rides this line rather than a cell each row re-answers — the adapter that owns a connection binds the class, and a row carrying its own curve makes the effective attempts a product of two.
- Law: tenancy is NOT a column, on `transport/roots#RESOURCE`'s own reason — a binding isolates no tenant, the subscription's admitted profile and its resolved credential do — and a coordinate a row cannot express records the divergence on `degrade` rather than dropping the column.
- Law: the execution arm rides the row because it is a protocol fact, and NO row creates or owns an event loop. `KAFKA` is a blocking librdkafka client whose every blocking call releases the GIL, so it rides a `CapacityLimiter`-bounded `to_thread` lane with its delivery, rebalance, and settlement callbacks re-entering through one `BlockingPortalProvider`. `RABBITMQ` is blocking and single-threaded by contract, so it takes one dedicated worker per connection whose only inbound door is `add_callback_threadsafe`, and its pump calls `process_data_events` on its own cadence because deliveries and heartbeats dispatch nowhere else. `MQTT` needs no thread at all — `socket()` registers on the caller's own readiness and `loop_read`/`loop_write`/`loop_misc` run as bounded steps inside the task group. `NATS` is asyncio-native and composes directly, forfeiting the trio backend on its own `degrade`. `HTTP` rides the `transport/roots#RESOURCE` arm already bound.
- Law: `dataref` is ONE policy row per binding and never a global constant, because a threshold fixed estate-wide either strands the smallest transport or wastes the largest. `threshold` derives from the binding's own NEGOTIATED limit where the protocol negotiates one — NATS reads the live connection's `max_payload`, MQTT 5.0 the session's `Maximum Packet Size`, AMQP the per-link `max-frame-size` — and from the row's declared floor otherwise. `residence` binds at the composition root as a `transport/roots#STORE` port and refuses at admission when unbound rather than shipping a reference nothing resolves; `ref` IS the digest in the one `subject` spelling; `retain` names a `Retain` class and never a window; `dual` gates reference-alone shipping on the subscription's own `protocolsettings`, since the specification carries no capability negotiation.
- Law: `dataclassification` gates what crosses which binding, and `CLASSIFICATION_ROWS` is where that gate is DATA — one row per `execution/admission#CONTEXT` `Classification` grade carrying its `redact` transform and its `broker` reach, so a grade a binding cannot honor refuses AT that binding rather than at a later hop and a payload crossing without the redaction route is the exfiltration path the row forecloses. `SECRET` reaches no broker at all: its `broker` cell is empty, so every broker row refuses it by name while HTTP still carries it under the redaction the same row names.
- Entry: `lower(envelope, binding, mode)` is one entry over every protocol and both modes, folding the row's own codec and prefix off the table, and `raise_(message, binding)` is its inverse discriminating content mode by the row's own rule — HTTP and Kafka on the header prefix, AMQP and RabbitMQ on the content type's `application/cloudevents` stem. Both rail; neither takes a knob the row already answers.
- Auto: a binding a deployment cannot serve refuses on the `providers` OPEN axis as one `execution/admission#CONTEXT` descriptor row, never a boolean knob, because a knob re-mints the assumed consumer roster the open form forecloses. AMQP 1.0 is exactly that case in this branch: the row lowers and raises an `AMQPMessage` value and names no client, so a composition binding it refuses at admission with the axis named.
- Growth: a new protocol is one `BindingRow` with its `Dataref` row, reaching every `CLASSIFICATION_ROWS` `broker` cell that admits it; a new sensitivity grade is one `Classification` member at its admission owner with one `CLASSIFICATION_ROWS` row here; a new content mode is one `Content` member on the rows that hold it; a new protocol setting is one key on that row's slice; a new pushdown mechanism is one `Pushdown` value; a new execution arm is one `Arm` member with its lane law; a new residence is one port binding at the composition root.
- Boundary: protocol lowering, its policy rows, and payload residence only — the connection half seats at `[04]-[ADAPTER]` on this same page, so a row states the protocol fact and the lane realizes it. Composes — never re-mints — the message envelope, the format contract, the resilience curves, the store lane, and the hook registry. Rejected: a per-sink knob outside its row's `protocolsettings` slice; a `ce-` literal at a lowering site; a hand-spelled partition-key extractor beside `_default_key_mapper`; a global `dataref` threshold; a `retry` column on a transport row; a boolean capability knob where the `providers` axis refuses.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from enum import StrEnum
from typing import Final, Literal

from expression import Nothing, Option, Some
from expression.collections import Map
from msgspec import Struct

from cloudevents.core.bindings import amqp, http, kafka, rabbitmq
from cloudevents.core.bindings.kafka import PARTITIONKEY_ATTR

from rasm.runtime.admission import Classification
from rasm.runtime.event import Content, MessageEnvelope, Suffix
from rasm.runtime.faults import RuntimeRail
from rasm.runtime.journal import Retain
from rasm.runtime.receipts import Scrub
from rasm.runtime.roots import ResourceRef

# --- [TYPES] ----------------------------------------------------------------------------

# `Message` is the SDK's own per-protocol carrier union beside the two branch-owned shapes, so a lowering answers a
# transport-neutral value and no adapter type reaches this page.
type Message = http.HTTPMessage | kafka.KafkaMessage | amqp.AMQPMessage | rabbitmq.RabbitMQMessage | MqttMessage | NatsMessage
type Settings = Map[str, str]


class MqttMessage(Struct, frozen=True, gc=False):
    # branch-owned, because the distribution ships no MQTT binding: `properties` is the UNPREFIXED User Property pair
    # list `paho.mqtt.properties.Properties.UserProperty` carries, so an attribute name and a peer's own property name
    # share one namespace by construction.
    topic: str
    properties: tuple[tuple[str, str], ...]
    content_type: Option[str]
    payload: bytes


class NatsMessage(Struct, frozen=True, gc=False):
    # branch-owned on the same reason; `headers` gates on the server's own advertised header support, so binary mode
    # refuses at admission where that advertisement is absent rather than publishing a headerless message.
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
    # distinct families across the SDK bindings, beside the empty one MQTT carries. The member VALUE is the literal, so
    # no lowering site spells `"ce-"` and a fifth family cannot appear by typo.
    DASH = "ce-"
    UNDERSCORE = "ce_"
    QUALIFIED = "cloudEvents_"
    NONE = ""


class Arm(StrEnum):
    # NO row creates or owns a loop. `THREAD` is the GIL-releasing blocking client on a bounded lane; `PUMP` the
    # single-threaded client whose one worker owns the connection and whose only inbound door is a threadsafe
    # callback; `READY` the socket-first client stepped inside the caller's task group with no thread at all;
    # `NATIVE` the already-async client; `ABSENT` the protocol this branch lowers and cannot dial.
    THREAD = "thread"
    PUMP = "pump"
    READY = "ready"
    NATIVE = "native"
    ABSENT = "absent"


class Pushdown(StrEnum):
    BROKER = "broker"      # the routing attribute resolves at the broker: MQTT SUBSCRIBE, NATS wildcards, an exchange binding
    LINK = "link"          # AMQP link-source filters under a copy or move distribution mode
    CONSUMER = "consumer"  # no server-side mechanism; every expression evaluates after delivery


# --- [MODELS] ---------------------------------------------------------------------------


class ClassificationRow(Struct, frozen=True, gc=False):
    # handling law per sensitivity grade, transcribed meaning-identical from the estate seam. `redact` is the
    # receipts-owned `Scrub` transform vocabulary, distinct in NAME because it is a distinct concept — one spelling
    # over a grade and a transform resolves to whichever module imported last. `broker` names the
    # bindings a payload at this grade may cross AT ALL, derived nowhere else: an empty set is a total refusal every
    # broker row reads by name rather than a policy each adapter re-derives.
    grade: Classification
    redact: Scrub
    broker: frozenset[Binding]
    carries: str


CLASSIFICATION_ROWS: Final[Map[Classification, ClassificationRow]] = Map.of_seq(
    (row.grade, row)
    for row in (
        ClassificationRow(
            Classification.PUBLIC,
            redact="hash",
            broker=frozenset(Binding),
            carries="a fact whose payload is publishable as it stands, so every binding carries it and the transform only stabilizes identity",
        ),
        ClassificationRow(
            Classification.INTERNAL,
            redact="hash",
            broker=frozenset(Binding),
            carries="the default grade: estate-interior facts every admitted binding carries under the standing trust row",
        ),
        ClassificationRow(
            Classification.RESTRICTED,
            redact="mask",
            broker=frozenset({Binding.HTTP, Binding.KAFKA, Binding.RABBITMQ}),
            carries="a fact whose payload masks before it crosses, and only onto the bindings whose trust row this branch verifies per destination",
        ),
        ClassificationRow(
            Classification.SECRET,
            redact="drop",
            broker=frozenset(),
            carries="a fact no broker carries at all — the payload DROPS and only the attribute projection crosses, so the reference-carrying leg is the whole delivery",
        ),
    )
)


class Dataref(Struct, frozen=True, gc=False):
    # one row per binding and never a global constant: a threshold fixed estate-wide strands the smallest transport
    # or wastes the largest. `negotiated` marks the protocols whose limit is read off the LIVE session rather than
    # this floor — NATS `max_payload`, MQTT 5.0 `Maximum Packet Size`, AMQP `max-frame-size` — so the floor is the
    # refusal value when no session is open, never the operating value when one is.
    threshold: int
    negotiated: bool
    retain: Retain
    dual: bool


class BindingRow(Struct, frozen=True, gc=False):
    # whole protocol vocabulary in one shape. The `[CONSUMPTION_DESCRIPTOR]` five answer selection — `fits`,
    # `admit`, `lifetime`, `degrade`, with tenancy foreclosed because a binding isolates no tenant — and the
    # transport six answer engine behavior. `retry` is FORECLOSED for the family: `retries` names the class that
    # owns the schedule, since a row carrying its own curve makes the effective attempts a product of two.
    binding: Binding
    modes: frozenset[Content]
    prefix: Prefix
    routes_on: Option[str]
    settings: frozenset[str]
    pushdown: Pushdown
    arm: Arm
    dataref: Dataref
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
            modes=frozenset({Content.BINARY, Content.STRUCTURED, Content.BATCH}),
            prefix=Prefix.DASH,
            routes_on=Nothing,
            settings=frozenset({"headers", "method"}),
            pushdown=Pushdown.CONSUMER,
            arm=Arm.NATIVE,
            dataref=Dataref(threshold=8 << 10, negotiated=False, retain=Retain.OPERATIONAL, dual=False),
            fits="a webhook target or a synchronous ingress door, the one binding carrying the abuse-protection handshake",
            admit="`lower` over `cloudevents.core.bindings.http`, dialed through the `transport/roots#RESOURCE` http arm already bound",
            lifetime="the request; nothing survives the response and no subscription state accumulates here",
            deliver="at-most-once on a bare POST, at-least-once where the target answers and the producer re-drives",
            order="none across requests; a consumer orders on `sequence` alone",
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
            modes=frozenset({Content.BINARY, Content.STRUCTURED, Content.BATCH}),
            prefix=Prefix.UNDERSCORE,
            routes_on=Some(PARTITIONKEY_ATTR),
            settings=frozenset({"topicname", "partitionkeyextractor", "clientid", "acks"}),
            pushdown=Pushdown.CONSUMER,
            arm=Arm.THREAD,
            dataref=Dataref(threshold=1 << 20, negotiated=False, retain=Retain.OPERATIONAL, dual=True),
            fits="the durable, partitioned, replayable log every analytic and audit consumer reads, and the one binding carrying a registry-framed payload",
            admit="`lower` over `cloudevents.core.bindings.kafka`, produced through the `confluent-kafka` client on a bounded thread lane",
            lifetime="the topic's own retention; this branch commits offsets and deletes nothing",
            deliver="at-least-once by default, exactly-once inside a transaction whose offsets settle with the produce",
            order="total within a partition, so `partitionkey` decides what stays ordered and nothing orders across keys",
            settle="an explicit offset commit joined to the durable write, never an auto-commit that outruns it",
            replay="a seek to any retained offset, which is what makes this the audit-grade row",
            bound="`message.max.bytes` and `max.request.size`, both about a mebibyte",
            refuse="a payload past the frame budget with no bound residence, and a classification the topic's trust row does not carry",
            degrade=(
                "no server-side filtering, so every subscription expression evaluates consumer-side after the fetch",
                "headers cross as bytes and every attribute value stringifies, so a typed extension round-trips only through the roster's own codecs",
                "the SDK does not propagate headers onto the message handed to a delivery report, so producer-side evidence reads the envelope it sent",
            ),
        ),
        BindingRow(
            Binding.MQTT5,
            modes=frozenset({Content.BINARY, Content.STRUCTURED}),
            prefix=Prefix.NONE,
            routes_on=Some("topicname"),
            settings=frozenset({"topicname", "qos", "retain", "expiry", "userproperties"}),
            pushdown=Pushdown.BROKER,
            arm=Arm.READY,
            dataref=Dataref(threshold=64 << 10, negotiated=True, retain=Retain.EPHEMERAL, dual=True),
            fits="the edge and telemetry plane — many small producers on constrained links, filtered at the broker",
            admit="the branch-owned lowering onto MQTT 5.0 User Properties; the distribution ships no MQTT binding at all",
            lifetime="the session; a retained message outlives it only where the row's `retain` setting says so",
            deliver="QoS 0 at-most-once, QoS 1 at-least-once, QoS 2 exactly-once — the subscription's own setting decides",
            order="per topic within one session, and nothing across a reconnect",
            settle="PUBACK or PUBCOMP by QoS, deferred where manual acknowledgement is armed",
            replay="none beyond the retained message and the session's own queued window",
            bound="the session's negotiated Maximum Packet Size, read off the live connection rather than this floor",
            refuse="a payload past the negotiated packet size with no bound residence, and a classification the topic's trust row refuses",
            degrade=(
                "attributes ride UNPREFIXED User Property names, so a peer's own property namespace collides with an attribute name by construction",
                "a wildcard subscription filters on the topic alone, so every non-topic expression still evaluates consumer-side",
                "the client re-raises a callback fault into its own network loop, so the crossing rails its faults rather than letting one kill the pump",
            ),
        ),
        BindingRow(
            Binding.MQTT311,
            modes=frozenset({Content.STRUCTURED}),
            prefix=Prefix.NONE,
            routes_on=Some("topicname"),
            settings=frozenset({"topicname", "qos", "retain"}),
            pushdown=Pushdown.BROKER,
            arm=Arm.READY,
            dataref=Dataref(threshold=64 << 10, negotiated=False, retain=Retain.EPHEMERAL, dual=True),
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
            prefix=Prefix.QUALIFIED,
            routes_on=Nothing,
            settings=frozenset({"address", "linkname", "sendersettlementmode", "linkproperties"}),
            pushdown=Pushdown.LINK,
            arm=Arm.ABSENT,
            dataref=Dataref(threshold=128 << 10, negotiated=True, retain=Retain.OPERATIONAL, dual=True),
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
            prefix=Prefix.DASH,
            routes_on=Some("subject"),
            settings=frozenset({"subject"}),
            pushdown=Pushdown.BROKER,
            arm=Arm.NATIVE,
            dataref=Dataref(threshold=1 << 20, negotiated=True, retain=Retain.EPHEMERAL, dual=True),
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
            prefix=Prefix.DASH,
            routes_on=Some("routingkey"),
            settings=frozenset({"exchange", "routingkey", "deliverymode", "expiration"}),
            pushdown=Pushdown.BROKER,
            arm=Arm.PUMP,
            dataref=Dataref(threshold=128 << 10, negotiated=False, retain=Retain.OPERATIONAL, dual=True),
            fits="a work-queue and routed-fanout plane where the exchange binding does the filtering the consumer would otherwise pay for",
            admit="`lower` over `cloudevents.core.bindings.rabbitmq`, published through `pika`'s blocking channel on its own worker",
            lifetime="the queue's own policy; a durable queue outlives every connection that fed it",
            deliver="at-least-once under publisher confirms, at-most-once without them",
            order="per queue, and nothing across a requeue",
            settle="an explicit ack, nack, or reject carrying its own requeue disposition",
            replay="none; a redelivery is the requeue, not a rewind",
            bound="the negotiated frame max, 128 KiB by this client's default",
            refuse="a classification the exchange's trust row does not carry, and a payload past the frame budget with no bound residence",
            degrade=(
                "the client is single-threaded by contract, so one worker owns the connection and every inbound crossing is a threadsafe callback",
                "deliveries, timers, and heartbeats dispatch ONLY inside the pump, so a worker that stops pumping stops answering the broker entirely",
                "the SDK writes plain unencoded header strings here, so a non-ASCII attribute value crosses as whatever the field table encodes rather than percent-encoded",
                "`datacontenttype` rides the dedicated content-type property rather than a header, so it is the one attribute this row does not prefix",
            ),
        ),
    )
)

# derived rosters: a consumer selecting on capability reads THESE and never re-lists a binding set, so a
# seventh row joins every roster untouched.
BROKER_FILTERED: Final[frozenset[Binding]] = frozenset(row.binding for row in BINDINGS.values() if row.pushdown is not Pushdown.CONSUMER)
DIALABLE: Final[frozenset[Binding]] = frozenset(row.binding for row in BINDINGS.values() if row.arm is not Arm.ABSENT)
NEGOTIATED_BOUND: Final[frozenset[Binding]] = frozenset(row.binding for row in BINDINGS.values() if row.dataref.negotiated)


def lower(envelope: MessageEnvelope, /, *, binding: Binding, mode: Content, suffix: Suffix) -> RuntimeRail[Message]:
    # ONE entry over every protocol and both modes: the row answers prefix, codec, and routing key, so no caller
    # threads a knob the table already holds and a mode outside `row.modes` refuses here naming the row.
    ...


def raise_(message: Message, /, *, binding: Binding) -> RuntimeRail[MessageEnvelope]:
    # inverse leg, discriminating content mode by the ROW's own rule rather than one estate-wide test: `http` and
    # `kafka` read the header prefix, `amqp` and `rabbitmq` the content type's `application/cloudevents` stem, and
    # both MQTT rows read the presence of a user-property carrier at all.
    ...


def residence(row: BindingRow, ref: Option[ResourceRef], /) -> RuntimeRail[ResourceRef]:
    # `residence` binds at the composition root as a port; an unbound port REFUSES at admission rather than shipping
    # a `dataref` nothing resolves, which is the whole reason the column is a port and not a value.
    ...
```

## [03]-[EMISSION]

- Owner: `Emitter` is an `observe` subscription over `observability/hooks#HOOKS` fired facts and never an emit inside a domain fold — a producer fires its fact once and this owner projects it into an message envelope, so the domain never learns a transport exists and a composition with no emitter bound loses no fact. `Delivery` is what a fan across bound bindings answers, carrying accepted beside matched-duplicate as separate halves.
- Law: the modality is `OBSERVE` and nothing else. `VETO` inverts the announcement law an message envelope exists under, letting a broker refusal reject a domain operation that already happened; a raising observe tap becomes a `rejected` receipt while the emitter's own rail stays `Ok`, which is exactly the isolation a fact stream needs.
- Law: batches settle PER EVENT. Receipts carry `accepted` beside `duplicate` as separate halves because a matched duplicate under `(source, id)` is not an acceptance, and folding them erases the exactly-once evidence `Uniqueness` exists to carry. `sequence` survives batching and no re-batch reorders events inside one `source`; a batch past the row's own budget splits at the PRODUCER, since a relay re-framing one cannot re-sign it.
- Law: every long-tail state crosses a declared rail rather than a default. Poison messages route to the dead-letter address on the subscription's own slice; a redelivery is distinguished from a duplicate by `(source, id)` alone; an out-of-order arrival with `sequence` present reorders inside its `source` window and without it does not reorder at all; a stamp after its own observation drops to unmeasured and REFUSES rather than publishing a negative lag; an oversized payload after compression takes the row's `dataref` leg; a broker refusing an attribute name surfaces that name rather than the whole message; an extension name colliding with a native transport header refuses at the lowering; an absent `datacontenttype` under the JSON format defaults to that format's own declared payload type.
- Law: a never-shedding consumer closes by FLUSHING, never by cancelling the in-flight window — the drain stops admitting first, then awaits what is already in flight, so a fact accepted at the boundary is never lost to its own teardown.
- Law: `dataclassification` gates before the lowering and not after, so a classification a row cannot honor never reaches an encoder; `source` and `authcontext` are producer claims verified against the trust row before any routing decision reads them.
- Entry: `Emitter.bound(points, scope=...)` registers one subscription over a whole hook roster and returns the detacher, so a producer's entire point table is tapped at one grain and the emitter dies with the composition that bound it. `scope` is the composition whose points it reaches — the SAME `ScopeKey` the producer registered under — because two compositions embedding the runtime in one process partition point custody structurally, and an emitter bound at the default scope reaches none of an embedded composition's roster. `Emitter.project` is the one fact-to-envelope arm, so a new fact family is one projection row and no binding is edited.
- Auto: fan-out across bound bindings inherits the bounds the single delivery already takes — the row's own retry class on every hop, the lane's capacity on every thread arm — so it buys concurrency and never a second bound. Refused bindings shed no sibling's delivery, so the caller re-drives exactly what failed.
- Receipt: `Delivery` carries the two halves, the per-binding disposition, and the residence reference where the payload externalized; receipt SEMANTICS stay the producing surface's, on `transport/roots#STORE`'s own split.
- Growth: a new fact family is one `project` row; a new bound binding is one member of the emitter's set with no projection edited; a new long-tail state is one declared rail on this cluster and one arm on the fold that reads it; a new composition is one `ScopeKey` threaded through `bound`, never a sibling emitter.
- Boundary: hook-fact projection and delivery fan only. Mints no hook point, no receipt semantics, no retention window, and no client connection. Rejected: an emit inside a domain fold; a `VETO` subscription over a fact stream; accepted and matched-duplicate folded into one count; a re-batch at a relay; a drain that cancels the in-flight window.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Callable
from typing import Final

from expression import Option
from expression.collections import Block, Map
from msgspec import Struct

from rasm.runtime.event import MessageEnvelope, Uniqueness
from rasm.runtime.faults import BoundaryFault, RuntimeRail
from rasm.runtime.hooks import HookPoint
from rasm.runtime.receipts import DEFAULT_SCOPE, ScopeKey
from rasm.runtime.roots import ResourceRef

# --- [TYPES] ----------------------------------------------------------------------------

# projections are per FACT family and carry their own grammar decisions, so a new family is one row rather than a
# branch inside one emitter body.
type Project[P: Struct] = Callable[[P], RuntimeRail[MessageEnvelope]]

# --- [MODELS] ---------------------------------------------------------------------------


class Delivery(Struct, frozen=True, gc=False):
    # two HALVES, never one count: a matched duplicate under `(source, id)` is not an acceptance, and a single
    # total erases the exactly-once evidence the composite exists to carry. `externalized` names where a payload
    # past its row's threshold actually landed, so a consumer resolves the reference without re-deriving the store.
    accepted: Block[Uniqueness]
    duplicate: Block[Uniqueness]
    refused: Block[tuple[Binding, BoundaryFault]]
    externalized: Option[ResourceRef]


class Emitter(Struct, frozen=True, gc=False):
    # an OBSERVE subscriber and never an emit inside a domain fold. A veto here would let a broker refusal reject an
    # operation that already happened, which inverts the law an announcement exists under.
    projections: Map[str, Project[Struct]]
    bindings: frozenset[Binding]

    def bound(self, points: Block[HookPoint[Struct]], /, *, scope: ScopeKey = DEFAULT_SCOPE) -> RuntimeRail[Callable[[], None]]:
        # one subscription over the producer's WHOLE point table, answering the detacher the composition holds — the
        # roster-to-subscribe fold lives at the hook registry, so this owner hands it a `Block` and nothing more.
        # `scope` is the composition the producer REGISTERED under: the registry partitions point custody by that key,
        # so an emitter bound at the default scope over an embedded composition's roster subscribes to nothing at all
        # and the fact stream reads as an empty producer rather than as a missed binding.
        ...

    def project[P: Struct](self, point_id: str, payload: P, /) -> RuntimeRail[MessageEnvelope]:
        ...
```

## [04]-[ADAPTER]

- Owner: `BrokerLane` is the ONE connection owner for every dialable protocol and `ADAPTERS` the table it reads — membership shape, settlement join, producer guarantee, poll cadence, lane and portal need, in-flight window, dead-letter route, and drain law, one row per protocol. Protocol-specific machinery reaches it as a composition-bound `Client` port carrying six thunks and nothing else, so a seventh protocol is one `ADAPTERS` row beside one bound port: no adapter subclass, no per-protocol arm inside the lane, and no branch on `Binding` anywhere in the fold. `Consumption` is the caller's subscription coordinate and `Drained` the terminal evidence.
- Cases: `Pump` closes the loop vocabulary and each value is a protocol FACT the row states rather than a knob. `POLL` is the blocking librdkafka `poll`/`consume` on a `CapacityLimiter`-bounded `to_thread` lane, sound because every blocking C call releases the GIL. `WORKER` is the single-threaded `pika` connection whose one worker calls `process_data_events` on its own cadence and whose only inbound door is `add_callback_threadsafe`. `READY` is `paho`'s socket-first triple — `socket()` registers on the caller's own readiness and `loop_read`/`loop_write`/`loop_misc` run as bounded steps inside the task group, since `loop_start`'s daemon thread outlives every cancel scope. `NATIVE` is the already-async `nats` client. `REQUEST` is HTTP, where the request IS the crossing and no loop exists to bound.
- Cases: `Grouping` closes membership. `GROUP` is Kafka's cooperative-sticky consumer group: `on_assign` calls `incremental_assign` and `on_revoke`/`on_lost` call `incremental_unassign`, so a rebalance moves exactly the partitions that changed hands and every other member keeps fetching. `QUEUE` is one message to one member of a named set — a NATS queue group, an MQTT 5.0 shared subscription. `WORK` is RabbitMQ's competing consumers over one queue under `basic_qos` prefetch. `NONE` is a fan where every consumer sees every message, which is what makes a non-shared MQTT topic and a core NATS subject unfit for a work split.
- Law: NO lane creates or owns a loop, and `lifecycle` defaults `caller-owned`. `bound(group, ...)` composes every leg inside the caller's `anyio` task group, so the poll loop's lifetime IS that group's and a cancelled scope reaches a checkpoint rather than orphaning a thread. Sync clients ride the row's own `CapacityLimiter`, and every callback the client fires on its own thread re-enters through ONE `BlockingPortalProvider` — the portal is per lane and never per callback, since a provider minted inside a callback is a second loop owner in the shape the ban exists to foreclose. `Pump.NATIVE` composes under the anyio asyncio backend and forfeits trio on its own `degrade`, stated rather than assumed.
- Law: a settlement never outruns the durable write it stands for. `Settle.JOURNAL` rows disarm every automatic path at construction — Kafka takes `enable.auto.commit=false` beside `enable.auto.offset.store=false`, RabbitMQ takes `auto_ack=False`, JetStream takes an explicit acknowledgement policy — and the lane stores the offset only after `observability/journal#FACT` reports the write durable, then commits synchronously and reads the per-partition `.error` off the answered `TopicPartition` list. Committed offsets sit ONE PAST the message offset, so a lane storing the delivered offset replays the last message on every restart. Automatic commit is the deleted form outright: it acknowledges what a crash then loses, and that loss is invisible at both ends.
- Law: the producer guarantee is the row's, and a `deliver` claim better than at-least-once BUYS a boundary rather than asserting one. `Producing.IDEMPOTENT` arms the producer's own sequencing so a broker-side retry mints no duplicate. `TRANSACTIONAL` opens `init_transactions` once per lane and brackets each unit with `begin_transaction`, `send_offsets_to_transaction(positions, consumer_group_metadata())`, and `commit_transaction`, so the consumed offsets and the produced records settle as ONE fact and a read-process-write leg is exactly-once end to end. Raises inside the bracket read `KafkaException.args[0]`: `txn_requires_abort()` takes `abort_transaction` and re-drives, `fatal()` tears the lane down, and anything else rides the `RetryClass.BROKER` curve. `CONFIRMED` is publisher confirms, where a publish either round-trips or the composition declared at-most-once; `UNCONFIRMED` states that fire-and-forget in the open.
- Law: rebalance callbacks fire on whichever thread drove the poll, so they cross the portal and start NO work of their own. Each callback records the assignment delta as a value and returns; the task group reads it and reacts. Starting a fetch, a commit, or a journal write from inside a rebalance callback runs library work on the client's own thread under a lock the client holds, which is the deadlock the portal boundary exists to foreclose, and a callback that raises kills the network loop it fired on rather than surfacing.
- Law: backpressure is ONE bound, not two. `CapacityLimiter` bounds concurrent handler work and the row's `prefetch` bounds what the broker hands this member before it stops feeding — Kafka's fetch window, RabbitMQ's `basic_qos` count, MQTT's in-flight maximum, JetStream's batch — and the two are sized together so a saturated handler stops the broker rather than growing an unbounded in-memory queue behind it. Raising prefetch above the limiter buys latency the lane then pays as memory.
- Law: a poison message routes to the dead-letter address on the subscription's own `protocolsettings` slice and settles as a shed half on the receipt, never as an infinite redelivery. Redelivery is distinguished from duplication by `(source, id)` alone, so a redelivered fact the journal already holds settles as a matched duplicate and a genuinely new fact over identical bytes settles as an acceptance.
- Law: the drain FLUSHES and never cancels. `drained(deadline)` stops admitting first, then awaits the in-flight window, then flushes the producer — `flush` polls until the queue empties, so every pending delivery report lands and `purge` surfaces an unsent message as its own report rather than dropping it silently — then settles what the handlers finished, and only then closes. Cancelling the in-flight window instead loses facts already accepted at the boundary, which is exactly the loss the acceptance promised against.
- Entry: `BrokerLane.bound(group, binding, client, consumption)` is the one composition entry, answering the lane beside the detacher the caller holds; `published` is the one produce arm over a lowered `Message`, and `consumed` the one async iterator every handler drains. `drained(deadline)` is the terminal, and `transacted(unit)` brackets a read-process-write unit where the row's `producing` earns it. Every one rails, and none takes a knob its row already answers.
- Auto: ingress ADMITS through `execution/admission#TENANCY` and inherits nothing — a decoded message envelope carries no authority its transport happened to hold, so `source` and `authcontext` verify against the trust row before any routing decision reads them and a refused claim sheds that fact alone. Classification gates before the lowering on `CLASSIFICATION_ROWS`, so a grade a binding cannot honor never reaches an encoder.
- Auto: every dial and every attempt crosses the `RetryClass.BROKER` message envelope, so the failure window and the admission rate arrive already bound — an open circuit refuses without dialing and a throttle directive re-seats the lane's own bucket through `RateGate.directed`, which is where a librdkafka throttle event and a JetStream stall both land.
- Auto: the observability join is three `observability/metrics#METRIC` rows and no second emit path. `rasm.broker.ingest_lag` takes `Delivered.lag` — the `recordedtime` minus `time` this crossing measured, which is the reading collapsing the two stamps erases — and `rasm.broker.settled` beside `rasm.broker.shed` split acceptance, matched duplicate, and every export loss into two monotonic series keyed on the binding, so a fact that did not cross names its cause rather than vanishing between a produce and a receipt. Every one records off the same `Delivery`/`Drained` value the receipt carries, so the series and the log line cannot disagree about one crossing.
- Receipt: `Drained` carries what the teardown PROVED — the facts flushed, the settlements landed, the in-flight window that finished, and the shed halves with their causes — so a drain that lost nothing and a drain that shed are distinguishable rather than both reading as a clean close. Receipt semantics stay the producing surface's, on `transport/roots#STORE`'s own split.
- Growth: a new protocol is one `ADAPTERS` row beside one bound `Client`; a new membership shape is one `Grouping` member with its arm on the one membership fold; a new settlement join is one `Settle` member; a new producer guarantee is one `Producing` member with its bracket; a new loop cadence is one `Pump` member with its step; a new dead-letter route is one value on the subscription's slice.
- Boundary: connection lifetime, membership, settlement, and drain only. Mints no message envelope, no format, no retry curve, no failure window, no receipt semantics, and no hook point. Rejected: a lane creating a loop or a thread the caller's group does not own; `loop_start`'s daemon thread; a `BlockingPortalProvider` minted per callback; an automatic offset commit; a prefetch unpaired with its limiter; work started inside a rebalance callback; a drain that cancels the in-flight window; a per-protocol adapter class beside the one row-driven lane.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import AsyncIterator, Callable
from enum import StrEnum
from typing import Final, Protocol, Self

import anyio
from anyio import CapacityLimiter
from anyio.abc import TaskGroup
from anyio.from_thread import BlockingPortalProvider
from expression import Nothing, Option, Some
from expression.collections import Block, Map
from msgspec import Struct

from rasm.runtime.admission import RuntimeContext, TenantAdoption
from rasm.runtime.event import MessageEnvelope, Uniqueness
from rasm.runtime.faults import BoundaryFault, RuntimeRail
from rasm.runtime.journal import Fact
from rasm.runtime.resilience import RateGate, RetryClass

# `Binding`, `Content`, `Suffix`, `Message`, `BINDINGS`, `CLASSIFICATION_ROWS`, and `lower`/`raise_` are this
# module's [02]-[BINDING] owners and `Emitter` its [03]-[EMISSION] one — one module, three regions.

# --- [TYPES] ----------------------------------------------------------------------------

# six thunks a protocol's own package supplies, bound at the composition root and nothing more: `dial` opens the
# connection, `emit` hands it one lowered message, `step` advances that protocol's machine ONE bounded unit and
# answers what it delivered, `settle` acknowledges a settled batch, `flush` drains the producer's own queue, and
# `shut` closes. Every other protocol fact is row data, so the lane below branches on no `Binding` value at all.
type Dial = Callable[[], object]
type Emit = Callable[[object, Message], None]
type Step = Callable[[object, float], Block[Message]]
type SettleThunk = Callable[[object, Block["Delivered"]], None]
type Flush = Callable[[object, float], int]
type Shut = Callable[[object], None]


class Pump(StrEnum):
    # loop cadence as a protocol FACT, never a knob: each value names the ONE integration shape whose cancellation
    # reaches a checkpoint, so the shapes that orphan a thread or outlive a scope are unspellable here.
    POLL = "poll"        # a blocking poll on a bounded thread lane; every blocking call releases the GIL
    WORKER = "worker"    # one dedicated worker owning a single-threaded connection, entered threadsafe
    READY = "ready"      # socket readiness stepped inside the caller's task group with no thread at all
    NATIVE = "native"    # an already-async client composing directly under the asyncio backend
    REQUEST = "request"  # no loop exists: the request IS the crossing


class Grouping(StrEnum):
    NONE = "none"    # every consumer sees every message, so a work split needs a second mechanism
    GROUP = "group"  # broker-assigned partitions under a cooperative protocol
    QUEUE = "queue"  # the broker hands one message to one member of a named set
    WORK = "work"    # competing consumers over one queue, bounded by prefetch


class Settle(StrEnum):
    JOURNAL = "journal"    # the settlement JOINS a durable write and never outruns it
    BROKER = "broker"      # the broker's own acknowledgement is the whole settlement
    RESPONSE = "response"  # the response status IS the settlement


class Producing(StrEnum):
    TRANSACTIONAL = "transactional"  # consumed offsets and produced records settle as one fact
    IDEMPOTENT = "idempotent"        # broker-side retries mint no duplicate
    CONFIRMED = "confirmed"          # a publish round-trips or the composition declared at-most-once
    UNCONFIRMED = "unconfirmed"      # fire-and-forget, stated in the open


# --- [MODELS] ---------------------------------------------------------------------------


class Client(Struct, frozen=True, gc=False):
    # composition-bound protocol port. It carries thunks rather than a client object because the lane owns
    # LIFETIME and the package owns MECHANISM: binding a live client here would make the lane's task group the
    # second owner of a connection something else already opened.
    dial: Dial
    emit: Emit
    step: Step
    settle: SettleThunk
    flush: Flush
    shut: Shut


class Consumption(Struct, frozen=True, gc=False):
    # caller-supplied subscription coordinate: the addresses this lane reads, the membership name where the row's
    # `Grouping` takes one, and the dead-letter address a poison message routes to. Everything else about the
    # subscription is `protocolsettings` on the binding row, so this shape holds only what varies per subscriber.
    addresses: Block[str]
    member: Option[str] = Nothing
    dead_letter: Option[str] = Nothing


class Delivered(Struct, frozen=True, gc=False):
    # one raised message envelope beside the opaque settlement handle its protocol needs to acknowledge it — an offset, a
    # delivery tag, an ack subject. The handle stays OPAQUE so the lane settles without knowing the protocol, and
    # `lag` is the measured `recordedtime - time` this crossing observed rather than a stamp a consumer re-derives.
    envelope: MessageEnvelope
    handle: object
    lag: float


class Drained(Struct, frozen=True, gc=False):
    # what the teardown PROVED, so a drain that lost nothing reads differently from one that shed: the facts the
    # producer flushed, the settlements that landed, the in-flight window that finished, and each shed half with its
    # cause. A bare success verdict here reports a clean close for a teardown that dropped an accepted fact.
    flushed: int
    settled: Block[Uniqueness]
    finished: int
    shed: Block[tuple[Uniqueness, BoundaryFault]]


class AdapterRow(Struct, frozen=True, gc=False):
    # one protocol's whole connection vocabulary. `lane` names the limiter a thread-crossing arm borrows and is
    # `Nothing` where no thread crosses at all; `portal` marks the arms whose client fires callbacks on its own
    # thread; `prefetch` is the in-flight window the broker honors, sized WITH the limiter rather than beside it.
    binding: Binding
    pump: Pump
    grouping: Grouping
    settle: Settle
    producing: Producing
    lane: Option[str]
    portal: bool
    rebalanced: bool
    prefetch: int
    fits: str
    admit: str
    lifetime: str
    degrade: tuple[str, ...]


ADAPTERS: Final[Map[Binding, AdapterRow]] = Map.of_seq(
    (row.binding, row)
    for row in (
        AdapterRow(
            Binding.KAFKA,
            pump=Pump.POLL,
            grouping=Grouping.GROUP,
            settle=Settle.JOURNAL,
            producing=Producing.TRANSACTIONAL,
            lane=Some("broker.kafka"),
            portal=True,
            rebalanced=True,
            prefetch=1000,
            fits="the durable, partitioned, replayable log every analytic and audit consumer reads, and the one arm carrying an exactly-once read-process-write unit",
            admit="the synchronous client on a `CapacityLimiter`-bounded `to_thread` lane, cooperative-sticky assignment armed at construction, offsets stored only past the durable write",
            lifetime="the caller's task group; the poll loop ends with the scope and the group membership leaves on the close",
            degrade=(
                "delivery, rebalance, and settlement callbacks fire on whichever thread drove the poll, so every one crosses the portal and starts no work of its own",
                "a poll may answer an EVENT rather than a record, so `error()` reads before `value()` and a partition-eof arrives that way",
                "headers do not propagate onto the message handed to a delivery report, so producer-side evidence reads the envelope it sent",
                "the shipped asyncio layer is refused: it dials the running loop and answers asyncio futures, pinning every composition to one backend",
            ),
        ),
        AdapterRow(
            Binding.RABBITMQ,
            pump=Pump.WORKER,
            grouping=Grouping.WORK,
            settle=Settle.JOURNAL,
            producing=Producing.CONFIRMED,
            lane=Some("broker.rabbitmq"),
            portal=True,
            rebalanced=False,
            prefetch=64,
            fits="a work-queue and routed-fanout plane where the exchange binding does the filtering a consumer would otherwise pay for",
            admit="the blocking connection on one dedicated worker, publisher confirms armed at composition, `basic_qos` prefetch sized with the lane limiter",
            lifetime="the caller's task group; the worker owns the connection for exactly that scope",
            degrade=(
                "the connection is single-threaded by contract, so every inbound crossing is one threadsafe callback and no other member is touched off-thread",
                "deliveries, timers, and heartbeats dispatch ONLY inside the pump, so a worker that stops pumping stops answering the broker entirely",
                "`start_consuming` refuses re-entry from inside any callback, so the pump advances through bounded `process_data_events` steps instead",
                "a cancel on an `auto_ack=False` consumer auto-nacks undispatched deliveries, so the requeue is the shed and never a silent loss",
            ),
        ),
        AdapterRow(
            Binding.NATS,
            pump=Pump.NATIVE,
            grouping=Grouping.QUEUE,
            settle=Settle.JOURNAL,
            producing=Producing.IDEMPOTENT,
            lane=Nothing,
            portal=False,
            rebalanced=False,
            prefetch=256,
            fits="the low-latency subject-addressed plane, and through JetStream the durable one, reached with no thread lane at all",
            admit="the async client composing directly in the caller's task group, JetStream pull consumers under an explicit acknowledgement policy",
            lifetime="the caller's task group for the pull loop; the connection's own reader, ping, and flusher legs end on an explicit drain",
            degrade=(
                "the client is asyncio-locked whole, so this arm forfeits the trio backend and composes under the asyncio one alone",
                "its internal legs are loop-level tasks rather than children of the caller's group, so teardown is an explicit drain and not a cancelled scope",
                "each subscription is a bounded queue whose overflow reaches the error callback as a slow-consumer fact, so the shed is evidence and never silence",
                "the payload ceiling is the connection's advertised maximum, read live, so an oversized fact takes the reference-carrying leg before any encode",
            ),
        ),
        AdapterRow(
            Binding.MQTT5,
            pump=Pump.READY,
            grouping=Grouping.QUEUE,
            settle=Settle.BROKER,
            producing=Producing.CONFIRMED,
            lane=Nothing,
            portal=False,
            rebalanced=False,
            prefetch=20,
            fits="the edge and telemetry plane — many small producers on constrained links, filtered at the broker",
            admit="the socket-first triple stepped inside the caller's task group, manual acknowledgement armed so a settlement can defer past the callback",
            lifetime="the caller's task group; the session outlives it only where the subscription's own retain setting says so",
            degrade=(
                "the daemon-thread loop is refused outright: it outlives every cancel scope and fires callbacks nothing can join",
                "a callback raise propagates out of the network loop, so the crossing rails its own faults rather than letting one kill the pump",
                "a publish never blocks and never raises on overflow — it answers a queue-size code — so the shed reads off the returned handle and never off an exception",
                "keepalive liveness is the misc step's alone, so a lane that stops stepping stops observing its own disconnection",
            ),
        ),
        AdapterRow(
            Binding.MQTT311,
            pump=Pump.READY,
            grouping=Grouping.NONE,
            settle=Settle.BROKER,
            producing=Producing.UNCONFIRMED,
            lane=Nothing,
            portal=False,
            rebalanced=False,
            prefetch=20,
            fits="a legacy broker or device fleet that never negotiated 5.0, reached without forking the producer",
            admit="the same socket-first triple, structured mode only, with no property surface at all",
            lifetime="the session, on the 5.0 row's own law",
            degrade=(
                "no shared subscription exists, so every consumer sees every message and a work split needs a second mechanism entirely",
                "no message expiry and no reason codes, so an expired fact drops at the consumer rather than at the broker",
                "the property surface is absent, so binary mode is unspellable and every attribute rides inside the body",
            ),
        ),
        AdapterRow(
            Binding.HTTP,
            pump=Pump.REQUEST,
            grouping=Grouping.NONE,
            settle=Settle.RESPONSE,
            producing=Producing.CONFIRMED,
            lane=Nothing,
            portal=False,
            rebalanced=False,
            prefetch=0,
            fits="a webhook target or a synchronous ingress door, the one arm carrying the abuse-protection handshake",
            admit="the `transport/roots#RESOURCE` http arm already bound, so this lane opens no client of its own",
            lifetime="the request; nothing survives the response and no subscription state accumulates",
            degrade=(
                "no loop and no membership: a delivery is one request, so ordering, replay, and group settlement are all absent by construction",
                "the target's published rate is the only pacing there is, so the handshake answer re-seats the lane's own bucket",
            ),
        ),
    )
)

# derived: a protocol whose arm crosses a thread borrows a limiter and re-enters through a portal, so the two rosters
# a composition sizes are comprehensions over the one table rather than hand-listed sets a seventh row misses.
THREADED: Final[frozenset[Binding]] = frozenset(row.binding for row in ADAPTERS.values() if row.lane.is_some())
PORTALED: Final[frozenset[Binding]] = frozenset(row.binding for row in ADAPTERS.values() if row.portal)
JOURNAL_SETTLED: Final[frozenset[Binding]] = frozenset(row.binding for row in ADAPTERS.values() if row.settle is Settle.JOURNAL)

# --- [SERVICES] -------------------------------------------------------------------------


class BrokerLane(Struct, frozen=True, gc=False):
    # ONE lane over every protocol, driven by its row and its bound port. `portal` is per LANE: a provider minted
    # inside a callback is a second loop owner in exactly the shape D16 forecloses, and one provider serves every
    # callback the client fires for this connection's whole life.
    row: AdapterRow
    client: Client
    consumption: Consumption
    context: RuntimeContext
    limiter: Option[CapacityLimiter]
    portal: Option[BlockingPortalProvider]

    @classmethod
    async def bound(
        cls, group: TaskGroup, binding: Binding, client: Client, consumption: Consumption, context: RuntimeContext, /
    ) -> RuntimeRail[Self]:
        # ONE composition entry. Every leg starts on the CALLER's group, so the poll loop's lifetime is that
        # group's and `lifecycle` stays caller-owned — a lane starting its own group or thread would outlive the
        # scope that opened it and answer a broker nobody is reading.
        ...

    async def published(self, envelope: MessageEnvelope, /, *, mode: Content, suffix: Suffix) -> RuntimeRail[Uniqueness]:
        # one produce arm over every protocol: `lower` answers the message, the row's `producing` decides the
        # boundary around it, and the composite answers so the caller keys its own dedup off the same value the
        # settlement will.
        ...

    def consumed(self) -> AsyncIterator[Delivered]:
        # ONE handler surface. Each step is bounded by the row's `pump`, so cancellation reaches a checkpoint
        # between steps and never inside a provider call; the iterator ends when the caller's scope does.
        ...

    async def settled(self, delivered: Block[Delivered], /) -> RuntimeRail[Block[Uniqueness]]:
        # `Settle.JOURNAL` rows await the durable write FIRST and settle second, so a commit never stands for a fact
        # no journal took. Committed offsets sit one past the delivered offset, which is why the handle stays the
        # protocol's own opaque value and never an offset this owner arithmetics.
        ...

    async def transacted[T](self, unit: Callable[[Block[Delivered]], RuntimeRail[T]], /) -> RuntimeRail[T]:
        # read-process-write bracket, reachable only where the row's `producing` is TRANSACTIONAL: consumed
        # positions and produced records settle as ONE fact. An abort-requiring raise re-drives the whole unit, a
        # fatal one tears the lane down, and every other raise rides the `RetryClass.BROKER` curve.
        ...

    async def drained(self, /, *, deadline: float) -> RuntimeRail[Drained]:
        # FLUSH, never cancel: stop admitting, await the in-flight window, flush the producer until its queue empties
        # so every delivery report lands, settle what the handlers finished, then close. A cancelled window loses
        # facts already accepted at the boundary, which is the one loss an acceptance promised against.
        ...

    def _admitted(self, envelope: MessageEnvelope, /) -> RuntimeRail[MessageEnvelope]:
        # ingress ADMITS and inherits nothing: the tenant claim, the asserted principal, and the `source` claim all
        # verify against the trust row before any routing decision reads them, so a decoded message envelope never carries
        # authority its transport happened to hold and a refused claim sheds that fact alone.
        ...
```

## [05]-[RESEARCH]

(none)

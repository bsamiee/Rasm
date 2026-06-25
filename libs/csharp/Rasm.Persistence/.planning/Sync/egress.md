# [PERSISTENCE_EGRESS]

Rasm.Persistence fans the one op-log changefeed to external sinks with the requested delivery guarantee, dead-letter capture, retriable-row redrive, and cursor replay — without minting a second changefeed. `EgressSink` is the `[SmartEnum<string>]` sink axis (webhook, NATS, Kafka, RabbitMQ, Pulsar, Arrow-Flight-push, gRPC-stream) carrying the `SinkBinding` each sink dials and the `WireSchema` governance posture its payload rides; `DeliveryGuarantee` is the `[SmartEnum<string>]` semantics axis whose `Settle` fold IS the delivery ceremony (fire-and-forget, await-durable-ack, or producer-transaction), so the guarantee is the carrier that selects the algebra and never a flag the pump branches on; `WireSchema` is the `[SmartEnum<string>]` payload-evolution axis whose governed rows mount a `Confluent.SchemaRegistry.Serdes.*` serde (Avro/Protobuf/JSON-Schema) on the broker value slot through `SerdeRail.Mount` while `Schemaless` rides the CloudEvents-over-snapshot body; `SinkScope` filters the settled `Version/provenance#LINEAGE_CDC` `CdcScope` lineage stream plus the `FrozenSet` entity-kind and op-kind allow-sets; `EgressPump` drains the redaction-aware `CdcEnvelope` stream past a durable `SyncCursor`, projects each redacted envelope into a CloudEvents v1.0 event carrying the `CdcEnvelope.Payload` in `Data`, delivers it under the guarantee, advances the cursor past each durably-settled entry (`egress.acked` for a durable broker ack, `egress.delivered` for the at-most-once lossy advance), folds a non-settled entry's broker `DeliveryAck` into a typed `EgressDeadLetter`, and re-delivers the durable RETRIABLE dead-letter rows through `Redrive` so a transient broker failure is eventually delivered rather than silently dropped past the advanced cursor. Each of the four wire protocols is a distinct `SinkBinding` whose durable ack folds into the one closed `DeliveryAck` family — `Confluent.Kafka` `ProduceAsync` → `DeliveryResult` (`FromResult`), `NATS.Net` JetStream `INatsJSContext.PublishAsync` → `PubAckResponse` (`FromPubAck`), `RabbitMQ.Client` v7 `BasicPublishAsync` publisher-confirm (`FromConfirm`), and `DotPulsar` `IProducer.Send` → `MessageId` (`FromPulsarSend`) — while `CloudNative.CloudEvents`/`.Kafka`/`.SystemTextJson` carry the standard envelope and the broker binding; `LineageCdc.Feed`/`CdcEnvelope`/`CdcScope`/`RedactorKind`, `OpLog`/`SyncCursor`, `Query/rail#ARROW_EGRESS` `ArrowEgress`, the `Query/rail#BULK_LANE` `StoreFact.BulkShed` shed fact, the AppHost `OutboundHop`, `ClockPolicy`, and `ReceiptSinkPort` arrive settled. Each webhook/gRPC egress registers as a keyed AppHost `OutboundHop` through the `Sync/egress ⇄ Rasm.AppHost/Runtime # [PORT]: keyed OutboundHop egress` seam — one of three keyed-hop consumers the AppHost `[ONE_OUTBOX_EGRESS_SPINE]` branch binds over the one op-log (this egress, the outbox relay, and the durable-orchestration step dispatch) draining the same `CdcEnvelopeWire` CloudEvents envelope; the Python runtime-transport leg decodes the CloudEvents event whose `Data` is the redacted op payload.

Wire posture: this page projects the redacted CDC stream onto the CloudEvents wire — the event carries the `CdcEnvelope`-projected payload, the `redacted`/`redactor`/`classification`/`traceparent` extension attributes, and the partition key, so the egress consumers (`python:runtime/transport`, external brokers) decode the one CloudEvents envelope, never a re-minted shape; the redaction is the settled `LineageCdc.Feed`/`ExportProof` masking and the `redactor` extension attribute names which redactor masked the row, so an out-of-authority payload crosses masked-and-stamped rather than raw, and a consumer renders an HMAC-pseudonymized field distinctly from an erased one.

## [01]-[INDEX]

- [01]-[EGRESS_SINK]: the sink axis with its four wire-protocol `SinkBinding` rows and per-protocol `DeliveryAck.From*` folds, the `WireSchema` registry-serde governance, the `DeliveryGuarantee.Settle` ceremony, the scope filter, the CloudEvents envelope, and the typed `DeliveryAck`/`EgressDeadLetter` with its `Advances`/`Retriable` discriminants.
- [02]-[EGRESS_PUMP]: the op-log-drain fold past a durable cursor, the durable-ack/lossy-advance split, the cursor replay, the latched shed throttle, the dead-letter capture, and the `Redrive` re-delivery of retriable rows.

## [02]-[EGRESS_SINK]

- Owner: `EgressSink` the `[SmartEnum<string>]` sink axis under the `SyncKeyPolicy` ordinal accessor, each row carrying `Broker`, the `SinkBinding` it dials, the `ContentMode` it frames in, and the `WireSchema` evolution posture its payload rides; `SinkBinding` the wire-protocol row family (`KafkaTopic`/`JetStream`/`AmqpChannel`/`PulsarTopic`/`OutboundHop`/`FlightStream`) so each protocol's publish-and-ack surface is one row, never a string the pump branches on; `WireSchema` the `[SmartEnum<string>]` payload-governance axis (`Schemaless`/`Avro`/`Protobuf`/`JsonSchema`) carrying its `Compatibility` level and `SchemaType`; `RegistryGovernance` the shared `Confluent.SchemaRegistry` control-plane seam (one `CachedSchemaRegistryClient`, `SubjectNameStrategy`, `SchemaIdSerializerStrategy`, `RuleRegistry`) and `SerdeRail` the value-slot serde mount; `DeliveryGuarantee` the `[SmartEnum<string>]` delivery-semantics axis whose `Settle` fold owns the per-guarantee delivery ceremony; `ProducerTxn` the exactly-once producer-transaction transformer (`Seal` commits-with-offset or aborts off the settled ack); `SinkScope` the lineage-scope-plus-entity-kind-plus-op-kind filter over the settled `CdcScope` (a CDC consumer subscribes to a subset of write verbs, so a "material changes only" sink and an audit-everything sink are two row sets, never two surfaces); `DeliveryAck` the closed `[Union]` broker-outcome family (`Persisted`/`Indeterminate`/`Refused`) folding the Kafka `DeliveryResult.Status` (`FromResult`), the NATS JetStream `PubAckResponse` (`FromPubAck`), the RabbitMQ publisher-confirm (`FromConfirm`), the Pulsar `Send` `MessageId` (`FromPulsarSend`), and the webhook status-class (`FromHttp`) into one outcome carrying the committed `TopicPartitionOffset` or the whole broker `Error`, with `Advances` (the cursor-advance discriminant) and `Retriable` (the redrive-eligibility discriminant) the two closed predicates the pump folds read; `Egress.Envelope` the CloudEvents projection of a redacted `CdcEnvelope`; `EgressDeadLetter` the typed dead-letter record carrying the whole failing `CdcEnvelope` the redrive re-delivers; `EgressFact` with `EgressFactKind` the page-wide fact stream and its slot factories.
- Cases: `EgressSink` webhook | nats | kafka | rabbitmq | pulsar | arrow-flight-push | grpc-stream — the `Broker`/`SinkBinding`/`ContentMode`/`WireSchema` quad is row data per sink, not a parallel type; `SinkBinding` kafka-topic | jetstream | amqp-channel | pulsar-topic | outbound-hop | flight-stream; `WireSchema` schemaless (the CloudEvents-over-snapshot body) | avro | protobuf | json-schema (each governed row a registry serde with a `Compatibility` evolution level); `DeliveryGuarantee` at-most-once | at-least-once | exactly-once; `SinkScope` carries the `CdcScope` lineage filter the `LineageCdc.Feed` applies plus the `FrozenSet` entity-kind AND op-kind (`cdc.Kind.Key`) allow-sets the pump refines on at O(1); `DeliveryAck` three broker outcomes — `Persisted` (the sole advancing case, carrying the committed `TopicPartitionOffset`, or `Offset.Unset` for a non-Kafka durable ack and an HTTP 2xx; `DeliveryAck.Lossy` is the at-most-once fire-and-forget advance settling `egress.delivered` not `egress.acked`), `Indeterminate` (a Kafka `NotPersisted`/`PossiblyPersisted`, a JetStream `ApiError`, an AMQP confirm-timeout, a Pulsar transient, an HTTP 5xx — RETRIABLE, the dead-letter holds it at the named `DeliveryAck.IndeterminateCode` and `Redrive` re-delivers it), `Refused` (a fatal broker `Error`, a `ProducerFencedException`, an unroutable AMQP `basic.return`, an HTTP 4xx — terminal, the dead-letter is the durable evidence, never re-driven); `Ack.Retriable`/`Ack.Advances` are the closed discriminants the redrive and cursor folds read; `EgressFactKind` delivered | acked | dead-letter | replay | shed, every slot minted by a real pump path.
- Entry: `public static CloudEvent Envelope(CdcEnvelope cdc)` projects a redaction-aware `CdcEnvelope` into a CloudEvents v1.0 event carrying the redacted `cdc.Payload` in `Data` (as `ReadOnlyMemory<byte>`, no defensive copy), the `redacted`/`redactor`/`classification` flags, the CNCF-standard `sequence` total-order extension, and the `traceparent`/`tracestate` from `cdc.Trace` as typed extension attributes, plus the partition key from the entity key — sink-agnostic and codec-agnostic because the `ContentMode`/`JsonEventFormatter` bind belongs at the Kafka/webhook seam (`EgressDrainPort.SinkOf` via `ToKafkaMessage(sink.Mode)`), not on the projector; delivery is `public IO<DeliveryAck> DeliveryGuarantee.Settle(IO<DeliveryAck> produce, ProducerTxn txn)` — the guarantee carrier IS the algebra, dispatching the per-row ceremony over the settled ack, so there is no `Deliver` forwarder and no guarantee-flag branch; the `ProducerTxn` transformer is the exactly-once producer-transaction `Seal` (commit-with-offset on a `Persisted` ack, abort otherwise) the Kafka sink supplies and the at-least/at-most rows bind as `ProducerTxn.None`.
- Auto: the egress reuses the one op-log changefeed through `LineageCdc.Feed` — the feed already filters by lineage scope and projects the redaction-aware payload but pushes nowhere, so this owner is purely the drain-and-push leg over its `CdcEnvelope` stream; each envelope projects into a CloudEvents v1.0 event with required `Id`/`Source`/`Type`/`Time`, the redacted `cdc.Payload` in `Data`, the `redacted`/`redactor`/`classification` extension attributes so a downstream consumer reads whether the payload was masked, by which redactor, and at what classification, the `sequence` extension attribute as the total order, and the `traceparent`/`tracestate` extension attributes so a consumer extract-and-continues the originating span (realizing `ONE_DISTRIBUTED_TRACE` across the egress), and the partition key set through `Partitioning.SetPartitionKey` from the entity key so co-keyed changes preserve per-key ordering on one partition; the `SinkBinding` row dials the transport at the `EgressDrainPort.SinkOf` seam — the Kafka binding composes `cloudEvent.ToKafkaMessage(sink.Mode, port.Formatter)` so attributes stay in headers and a broker filters without parsing the body, the webhook/gRPC bindings register as keyed AppHost `OutboundHop` rows so retry/backoff/deadline are the hop owner's concern, the Arrow-Flight-push binding rides the settled `Query/rail#ARROW_EGRESS` `ArrowEgress` Flight stream for a columnar bulk egress, and the NATS/RabbitMQ bindings ride their broker subjects; `DeliveryGuarantee.Settle` IS the ceremony — `AtMostOnce` fires the produce and folds any non-advancing outcome to `DeliveryAck.Lossy` so the cursor always advances (the lossy row settling `egress.delivered`, never falsely claiming a durable ack), `AtLeastOnce` awaits the durable `DeliveryAck` and advances the cursor only on `Persisted` (settling `egress.acked`), `ExactlyOnce` binds the settled ack into `ProducerTxn.Seal` (the Kafka `InitTransactions`/`BeginTransaction`/`SendOffsetsToTransaction`/`CommitTransaction` binding the CONSUMED `SyncCursor` offset on a `Persisted` ack, `AbortTransaction` on every non-advancing ack) so the produce and the cursor advance commit atomically; a non-advancing outcome on the at-least/exactly rows captures the entry in the durable `EgressDeadLetter` and `Redrive` re-delivers the retriable rows; `SinkScope` carries the settled `LineageCdc.Feed` lineage scope so a sink's redaction is the settled filter, never a second redactor.
- Receipt: every receipt is one `EgressFact` minted through its named slot factory so the slot fields are load-bearing, not stubbed zeros — `egress.acked` carries the committed `TopicPartitionOffset` and the advanced `Cursor` (a durable broker confirmation, or a `Redrive` success clearing a quarantined row); `egress.delivered` carries the entity key and cursor for the at-most-once fire-and-forget lossy advance, DISTINCT from `egress.acked` so the receipt never claims durability the lossy path lacks (the slot is a real pump path, not a phantom); `egress.dead-letter` carries the broker `Error` code-and-message in `Detail` and the redelivery `Attempts` ordinal; `egress.replay` carries the replayed-from sequence and the real `<redrained:n>` count; `egress.shed` carries the REAL `Elapsed`-at-shed measured from `clocks.Mark`, never `Duration.Zero` — five `EgressFactKind` slots over one stream, never five parallel buckets; the typed `EgressDeadLetter(Entry, Sink, Guarantee, Ack, Attempts, At)` carries the whole failing `CdcEnvelope` so `Redrive` re-projects and re-delivers it, with the attempt count threaded from the redelivery store; an at-least-once delivery without a durable `Persisted` ack settles as a typed dead-letter the redrive re-pumps, never silent, never permanently dropped.
- Packages: Confluent.Kafka, Confluent.SchemaRegistry, Confluent.SchemaRegistry.Serdes.Avro, Confluent.SchemaRegistry.Serdes.Protobuf, Confluent.SchemaRegistry.Serdes.Json, Chr.Avro, Chr.Avro.Binary, Chr.Avro.Confluent, NATS.Net, RabbitMQ.Client, DotPulsar, CloudNative.CloudEvents, CloudNative.CloudEvents.Kafka, CloudNative.CloudEvents.SystemTextJson, Apache.Arrow.Flight, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Rasm.AppHost (project).
- Growth: a new sink is one `EgressSink` row plus its `SinkBinding` row (and a per-protocol `DeliveryAck.From*` ack fold); a new delivery semantic is one `DeliveryGuarantee` row plus one `Settle` arm; a new wire schema is one `WireSchema` row carrying its `Compatibility`/`SchemaType` plus one `SerdeRail.Mount` arm; a new broker-outcome class is one `DeliveryAck` case (its `Advances`/`Retriable` discriminant placing it on the advance, quarantine, or redrive path with every `Match` site breaking at compile time); a new scope filter is one column on `SinkScope`; a new evidence bucket is one `EgressFactKind` row; a new redrive disposition is one arm on the `Redrive` ack `Match`; zero new surface — a second changefeed, a parallel envelope shape, a per-sink CDC feed, a per-guarantee delivery method, a per-protocol pump, a hand-rolled magic-byte schema-id framing, or a parallel dead-letter re-delivery worker is the deleted form because the egress drains the one op-log, projects the one CloudEvents envelope, the `Settle` fold owns every guarantee, `SerdeRail` mounts the one registry-governed serde per `WireSchema`, and `Redrive` re-pumps the one dead-letter substrate.
- Boundary: the egress drains the one op-log changefeed through `LineageCdc.Feed` and never mints a second feed — the feed already filters by lineage scope and projects the redacted payload, and this owner adds only the drain-and-push leg, so a parallel CDC feed or a trigger-based second changefeed is the deleted form; the wire payload is the redacted `CdcEnvelope.Payload` so an out-of-authority change crosses masked rather than raw — projecting the raw `OpLogEntry.Payload` past the redaction is the deleted form; the envelope is the one CloudEvents v1.0 projection — a hand-rolled CloudEvents JSON layout or a manual `ce_` Kafka header is the rejected form, the `JsonEventFormatter` encodes and `ToKafkaMessage(sink.Mode)` binds (the broker rows dial `ContentMode.Binary` so attributes ride headers, the webhook `OutboundHop` row dials `ContentMode.Structured` so the body carries the whole event); the redactor and classification ride DECLARED `CloudEventAttribute` extension attributes (`redactor`, `classification`) so the `redacted` boolean is never the only masking signal and the `CdcEnvelopeWire` redactor/classification fields decode without a re-derive; the delivery guarantee is the `Settle` fold's carrier so dispatch never branches on a guarantee flag and a `_`-armed switch over the three guarantees is the deleted form — a new guarantee breaks the generated `Switch` at compile time; the webhook/gRPC sinks register as keyed AppHost `OutboundHop` rows through the `Sync/egress ⇄ Rasm.AppHost/Runtime # [PORT]: keyed OutboundHop egress` seam so retry/backoff/deadline/correlation are the hop owner's concern (`docs/stacks/csharp/domain/resilience.md#ONE_OWNER`: exactly one retry owner per hop) and a second retry surface is the deleted form, the hop's bounded-retry budget bounding the `Redrive` loop; `DeliveryGuarantee.AtMostOnce` advances the cursor on the lossy ack and settles `egress.delivered` so the fire-and-forget receipt never falsely claims a durable ack; `DeliveryGuarantee.AtLeastOnce` advances the cursor only on a durable `Persisted` `DeliveryAck` (`egress.acked`) so redelivery after crash is the normal path and the consumer dedups on the entity content key; `DeliveryGuarantee.ExactlyOnce` threads the produce through the `ProducerTxn.Seal` transformer that reads the SETTLED ack — committing the produce in one Kafka transaction whose `SendOffsetsToTransaction` binds the CONSUMED `SyncCursor` offset (`EgressDrainPort.Txn` closes over the cursor, never the produced `TopicPartitionOffset` the ack carries) on `Persisted` and aborting on every non-advancing outcome — so the produce and the cursor advance are atomic and a blind `txn` wrap that cannot bind the consumed offset or abort on a `Refused` ack is the deleted form; the dead-letter discrimination is the `DeliveryAck` fold over the `FromResult` (Kafka `DeliveryResult.Status`) and `FromHttp` (webhook status-class) constructors — `Persisted` advances, `Indeterminate` (`NotPersisted`/`PossiblyPersisted`, a webhook 5xx) settles durably as RETRIABLE evidence `Redrive` re-delivers, and `Refused` (a `ProduceException.Error.IsFatal`/`IsBrokerError`, a webhook 4xx) settles durably as TERMINAL evidence never re-driven — both carry the attempt ordinal the redelivery store counts off the entity content key — so a hardcoded `"<egress-not-acked>"` fault with a literal `Attempts: 1` is the deleted form, the synthesized indeterminate detail rides the named `DeliveryAck.IndeterminateCode` (the ONE mint site, never a scattered `8260` literal), and the broker `Error` crosses WHOLE (never re-minted through `Error.New((int)code, reason)` that strips its identity) as the dead-letter evidence; a dead-letter store written but never re-driven — a transient failure permanently dropped past the advanced cursor — is the deleted form, and `Redrive` over the `Ack.Retriable` rows is the leg that closes it; the durable dead-letter substrate is NOT a second table — the `EgressDrainPort.DeadLetter`/`Quarantined`/`Resolve` hooks bind the settled `Sync/coordination#OUTBOX_TABLE` `CoordKind.DeadLetter` fenced-CAS cell (keyed by the egress sink plus the entity content key) under `TenantId` RLS so the egress CDC-delivery poison rides the one durable cross-process dead-letter owner the coordination store already arbitrates, and `Redrive` re-pumps it through the same `CoordStore.Read(Sweep)` dequeue the outbox relay dials — a parallel egress dead-letter table beside the coordination one is the deleted form; `Confluent.Kafka` is heavy and droppable to webhook/gRPC/Flight while retaining the CloudEvents envelope if broker scope is rejected; `SinkScope` carries the settled `LineageCdc.Feed` lineage scope and the `Version/retention#EXPORT` `ExportProof` redactor so a sink's masking is the settled filter, never a second redactor; the AppHost `[ONE_OUTBOX_EGRESS_SPINE]` branch binds three keyed `OutboundHop` consumers over the one op-log — this Persistence egress, the AppHost `Wire/outbox` relay, and the AppHost `Runtime/orchestration` durable-orchestration step dispatch — each draining the same `CdcEnvelopeWire` CloudEvents envelope as the hop payload, so a second outbox owner, a relay-side re-pack, or a per-consumer changefeed is the drift defect and the CloudEvents projection is the one wire payload across all three consumers; the Python runtime-transport leg decodes the CloudEvents event whose `Data` is the redacted op payload so the cross-language consumer reads the one envelope vocabulary; each broker is a distinct `SinkBinding` whose durable-ack surface folds into the one `DeliveryAck` family at exactly one boundary — `SinkBinding.JetStream` dials the NATS `INatsJSContext.PublishAsync` (Core `PublishAsync` on the at-most-once row) and folds its `PubAckResponse` through `FromPubAck` (the `Duplicate` flag the JetStream dedup-window idempotency the exactly-once-effective posture rides off the `Nats-Msg-Id` content key, since NATS carries no Kafka producer transaction), `SinkBinding.AmqpChannel` opens one `IChannel` per publishing path under `CreateChannelOptions(publisherConfirmationsEnabled, publisherConfirmationTrackingEnabled)` so a tracked `BasicPublishAsync` awaits the broker `basic.ack` (`FromConfirm`) and `mandatory: true` routes an unroutable `basic.return` to the dead-letter rather than silently dropping it — the AMQP `Tx*` legacy path is the rejected form where confirms exist, and `SinkBinding.PulsarTopic` awaits `IProducer.Send` → `MessageId` (`FromPulsarSend`) with `ProducerAccessMode.WaitForExclusive` electing the one WAL-leader producer per partition so a `ProducerFencedException` on the loser is the terminal `Refused` that triggers re-election — so a per-protocol pump or a collapsed single-error rail over the broker fault families is the deleted form; the `WireSchema` governance is the payload-evolution seam — a `Schemaless` sink frames the CloudEvents-over-MessagePack/CBOR body (the self-describing snapshot carries its own shape) while a governed sink mounts its `Confluent.SchemaRegistry.Serdes.*` serde on the broker value slot through `SerdeRail.Mount`, sharing one `CachedSchemaRegistryClient`, registering the writer schema out-of-band (`AutoRegisterSchemas = false`, `RegisterSchemaWithResponseAsync` under the row's `Compatibility`) so an incompatible producer schema is rejected at deploy rather than silently auto-registered, framing the id by `SchemaIdSerializerStrategy` (`Prefix` magic-byte default, `Header` when the id rides a `__value_schema_id` Kafka header beside the CloudEvents attributes), and wrapping per-field DEKs through the shared `RuleRegistry` CSFLE `DomainRule` against the `Store/encryption#KMS_PROVIDER` clients — the Avro leg over the `Apache.Avro` `ISpecificRecord` reference codec on the Kafka slot or the `Chr.Avro` `SchemaBuilder.BuildSchema<T>()` CLR-derived expression-compiled body off-Kafka, the Protobuf leg over the admitted `Google.Protobuf` `IMessage<T>` wire (the `Sync/egress ← Rasm.Compute` registry-governed Protobuf seam), the JSON-Schema leg with server-side validation — so a hand-rolled magic-byte framing, a per-message serde instance, an `AutoRegisterSchemas` durable producer, or a bespoke field-encryption pass outside the rule engine is the deleted form, the serde built once per stream and the multi-event op-log topic governed by `SubjectNameStrategy.TopicRecord` so a `BimCommitted` and a `GeometryRebaked` coexist under one topic each under its own subject.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------------

[SmartEnum<string>]
[KeyMemberEqualityComparer<SyncKeyPolicy, string>]
[KeyMemberComparer<SyncKeyPolicy, string>]
public sealed partial class EgressFactKind {
    public static readonly EgressFactKind Delivered = new("delivered");
    public static readonly EgressFactKind Acked = new("acked");
    public static readonly EgressFactKind DeadLetter = new("dead-letter");
    public static readonly EgressFactKind Replay = new("replay");
    public static readonly EgressFactKind Shed = new("shed");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<SyncKeyPolicy, string>]
[KeyMemberComparer<SyncKeyPolicy, string>]
public sealed partial class EgressSink {
    public static readonly EgressSink Webhook = new("webhook", broker: false, binding: SinkBinding.OutboundHop, mode: ContentMode.Structured, schema: WireSchema.Schemaless);
    public static readonly EgressSink Nats = new("nats", broker: true, binding: SinkBinding.JetStream, mode: ContentMode.Binary, schema: WireSchema.Schemaless);
    public static readonly EgressSink Kafka = new("kafka", broker: true, binding: SinkBinding.KafkaTopic, mode: ContentMode.Binary, schema: WireSchema.Avro);
    public static readonly EgressSink RabbitMq = new("rabbitmq", broker: true, binding: SinkBinding.AmqpChannel, mode: ContentMode.Binary, schema: WireSchema.Schemaless);
    public static readonly EgressSink Pulsar = new("pulsar", broker: true, binding: SinkBinding.PulsarTopic, mode: ContentMode.Binary, schema: WireSchema.Protobuf);
    public static readonly EgressSink ArrowFlightPush = new("arrow-flight-push", broker: false, binding: SinkBinding.FlightStream, mode: ContentMode.Binary, schema: WireSchema.Schemaless);
    public static readonly EgressSink GrpcStream = new("grpc-stream", broker: false, binding: SinkBinding.OutboundHop, mode: ContentMode.Binary, schema: WireSchema.Schemaless);

    public bool Broker { get; }

    public SinkBinding Binding { get; }

    public ContentMode Mode { get; }

    // The wire-schema governance the sink dials: a `Schemaless` row frames the CloudEvents body, a governed row
    // mounts its `Confluent.SchemaRegistry.Serdes.*` serde on the broker value slot through `SerdeRail.Mount`.
    public WireSchema Schema { get; }
}

[SmartEnum]
public sealed partial class SinkBinding {
    public static readonly SinkBinding KafkaTopic = new();      // Confluent.Kafka ToKafkaMessage(ContentMode.Binary) + ProduceAsync → DeliveryResult ack
    public static readonly SinkBinding JetStream = new();       // NATS INatsJSContext.PublishAsync → PubAckResponse durable ack (Core PublishAsync on the at-most-once row)
    public static readonly SinkBinding AmqpChannel = new();     // RabbitMQ IChannel.BasicPublishAsync under CreateChannelOptions publisher-confirm tracking
    public static readonly SinkBinding PulsarTopic = new();     // DotPulsar IProducer.Send → MessageId (WaitForExclusive leader on the exactly-once row)
    public static readonly SinkBinding OutboundHop = new();     // keyed AppHost OutboundHop; hop owns retry/backoff/deadline
    public static readonly SinkBinding FlightStream = new();    // Query/rail#ARROW_EGRESS ArrowEgress Flight DoPut stream
}

// The egress payload wire-schema governance axis. `Schemaless` rides the CloudEvents-over-MessagePack/CBOR
// snapshot body (self-describing, no registry); the three governed rows mount a `Confluent.SchemaRegistry.Serdes.*`
// serde on the broker value slot, share ONE `CachedSchemaRegistryClient`, register out-of-band under the row's
// `Compatibility` (never `AutoRegisterSchemas` on the durable changefeed), and frame the schema id by
// `SchemaIdSerializerStrategy` so a downstream consumer evolves the row safely. The carrier IS the codec — a sink
// reparameterizes its evolution posture from one row, never a parallel encoder beside the snapshot path; a new
// wire schema is one row plus one `SerdeRail` mount arm.
[SmartEnum<string>]
[KeyMemberEqualityComparer<SyncKeyPolicy, string>]
public sealed partial class WireSchema {
    public static readonly WireSchema Schemaless = new("schemaless", governed: false, level: Compatibility.None, type: SchemaType.Avro);
    public static readonly WireSchema Avro = new("avro", governed: true, level: Compatibility.BackwardTransitive, type: SchemaType.Avro);
    public static readonly WireSchema Protobuf = new("protobuf", governed: true, level: Compatibility.BackwardTransitive, type: SchemaType.Protobuf);
    public static readonly WireSchema JsonSchema = new("json-schema", governed: true, level: Compatibility.FullTransitive, type: SchemaType.Json);

    public bool Governed { get; }
    public Compatibility Level { get; }
    public SchemaType Type { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<SyncKeyPolicy, string>]
[KeyMemberComparer<SyncKeyPolicy, string>]
public sealed partial class DeliveryGuarantee {
    public static readonly DeliveryGuarantee AtMostOnce = new("at-most-once");
    public static readonly DeliveryGuarantee AtLeastOnce = new("at-least-once");
    public static readonly DeliveryGuarantee ExactlyOnce = new("exactly-once");

    // The carrier IS the algebra: Settle dispatches the per-guarantee delivery ceremony, never a flag
    // branch. The exactly-once transaction reads the SETTLED ack so it commits-with-offset on Persisted
    // and aborts on every non-advancing outcome — the txn transformer threads the offset and the abort
    // (Kafka SendOffsetsToTransaction/CommitTransaction vs AbortTransaction), never a blind wrap.
    public IO<DeliveryAck> Settle(IO<DeliveryAck> produce, ProducerTxn txn) =>
        Switch(
            state: (Produce: produce, Txn: txn),
            atMostOnce: static s => s.Produce.Map(static ack => ack.Advances ? ack : DeliveryAck.Lossy),
            atLeastOnce: static s => s.Produce,
            exactlyOnce: static s => s.Produce.Bind(s.Txn.Seal));
}

// The exactly-once producer-transaction transformer: the Kafka sink builds it from the SETTLED SyncCursor
// (EgressDrainPort.Txn closes over the consumed SyncCursor → IConsumerGroupMetadata offset), so Commit
// runs SendOffsetsToTransaction(cursor, group) + CommitTransaction binding the CONSUMED source offset (not
// the produced TopicPartitionOffset the ack carries) to the produced batch, and Abort runs AbortTransaction
// — the produce and the cursor advance commit atomically. Seal dispatches on Advances. At-most/at-least rows
// bind ProducerTxn.None (identity) because they advance off the ack alone, never a producer txn.
public readonly record struct ProducerTxn(Func<DeliveryAck, IO<DeliveryAck>> Commit, Func<DeliveryAck, IO<DeliveryAck>> Abort) {
    public static readonly ProducerTxn None = new(static ack => IO.pure(ack), static ack => IO.pure(ack));

    public IO<DeliveryAck> Seal(DeliveryAck ack) => ack.Advances ? Commit(ack) : Abort(ack);
}

// --- [MODELS] -------------------------------------------------------------------------------------

// The entity-kind AND op-kind allow-sets refine WITHIN the settled CdcScope lineage/classification/tenant
// filter — NEW dimensions the CdcScope does not carry, never a re-filter of what LineageCdc.Feed already
// scoped. A sink subscribes to a subset of entity kinds AND a subset of write verbs (a "material changes
// only" sink takes inserts+deletes, an audit sink takes every kind), so the op-kind set is the missing scope
// dimension a CDC consumer demands. Both sets are frozen so per-envelope admission is O(1) hash membership on
// the hot drain, never a Seq linear scan; an empty set admits every value the lineage scope already passed.
public sealed record SinkScope(CdcScope Lineage, FrozenSet<string> EntityKinds, FrozenSet<string> OpKinds) {
    public bool Admits(CdcEnvelope cdc) =>
        (EntityKinds.Count == 0 || EntityKinds.Contains(cdc.EntityKind))
        && (OpKinds.Count == 0 || OpKinds.Contains(cdc.Kind.Key));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record DeliveryAck {
    private DeliveryAck() { }

    // The broker outcome carries the position the cursor commits to: a Kafka TopicPartitionOffset or an
    // HTTP 2xx with no offset (Offset.Unset). Persisted is the sole advancing case; Indeterminate is the
    // RETRIABLE non-advance the redrive store quarantines and re-pumps, Refused the FATAL non-advance the
    // redrive store quarantines as terminal evidence — both durable, never silently dropped past the cursor.
    public sealed record Persisted(TopicPartitionOffset Offset) : DeliveryAck;     // DeliveryResult.Status == Persisted | HTTP 2xx
    public sealed record Indeterminate(string Reason) : DeliveryAck;               // NotPersisted | PossiblyPersisted | HTTP 5xx -> retriable, redrive
    public sealed record Refused(Error Fault) : DeliveryAck;                       // Error.IsFatal | IsBrokerError | HTTP 4xx -> terminal

    // The synthesized code for an Indeterminate dead-letter detail — the ONE mint site where the broker hands
    // back a status reason but no Error object (every Refused carries the broker Error WHOLE). Named, not a
    // literal scattered across call sites; the 8260 egress band sits one above the 8250 Sync/Retention band.
    public const int IndeterminateCode = 8260;

    // The one unset wire position a non-broker sink (webhook 2xx, fire-and-forget) commits to — shared by
    // Lossy, FromHttp, and every EgressFact slot with no Kafka offset, never re-constructed per call site.
    public static readonly TopicPartitionOffset UnsetOffset = new(string.Empty, Partition.Any, Offset.Unset);

    // The at-most-once advance: a fire-and-forget sink has no durable offset, so the lossy row carries the
    // unset position and always Advances so the cursor never blocks on a guarantee that accepts loss. Land
    // emits egress.delivered (not egress.acked) for the at-most-once advance because there is no durable ack.
    public static readonly DeliveryAck Lossy = new Persisted(UnsetOffset);

    // ProduceAsync returns DeliveryResult on success (Status + offset); a NotPersisted/PossiblyPersisted
    // status redelivers; a caught ProduceException.Error folds to Refused through Faulted.
    public static DeliveryAck FromResult(DeliveryResult<string?, byte[]> result) =>
        result.Status switch {
            PersistenceStatus.Persisted => new Persisted(result.TopicPartitionOffset),
            var status => new Indeterminate($"<broker:{status}>"),
        };

    // The NATS JetStream durable ack: INatsJSContext.PublishAsync awaits a PubAckResponse whose null Error is
    // the durable confirmation (Stream/Seq committed, the Duplicate flag the JetStream dedup-window idempotency
    // the exactly-once-effective posture rides off the Nats-Msg-Id content key), while an ApiError folds to a
    // retriable Indeterminate. NATS carries no Kafka offset, so the advance rides the op-log sequence (UnsetOffset).
    public static DeliveryAck FromPubAck(PubAckResponse ack) =>
        ack.Error is null
            ? new Persisted(UnsetOffset)
            : new Indeterminate($"<jetstream:{ack.Error.Code}:{ack.Error.Description}>");

    // The RabbitMQ publisher-confirm: under CreateChannelOptions(publisherConfirmationsEnabled,
    // publisherConfirmationTrackingEnabled) a tracked BasicPublishAsync awaits the broker basic.ack and surfaces
    // a basic.nack as a throw (→ Faulted/Refused at the sink) — so a settled (acked, !returned) confirm Persists,
    // a mandatory-unroutable basic.return is a terminal Refused, and an untracked timeout is a retriable Indeterminate.
    public static DeliveryAck FromConfirm(bool acked, bool returned) =>
        returned ? new Refused(Error.New(IndeterminateCode, "<amqp-unroutable-return>"))
        : acked ? new Persisted(UnsetOffset)
        : new Indeterminate("<amqp-confirm-timeout>");

    // The DotPulsar send ack: an awaited IProducer.Send returns the durable MessageId (Ledger:Entry:Partition
    // committed) → Persisted, a ProducerFencedException on a WaitForExclusive loser → Refused (re-election), and
    // any other transient DotPulsarException → Indeterminate. The MessageId carries no Kafka offset (UnsetOffset).
    public static DeliveryAck FromPulsarSend(Fin<MessageId> sent) =>
        sent.Match(
            Succ: static _ => new Persisted(UnsetOffset),
            Fail: static fault => fault.Exception is { Case: ProducerFencedException }
                ? (DeliveryAck)new Refused(fault)
                : new Indeterminate($"<pulsar:{fault.Message}>"));

    // The webhook/gRPC HTTP fold the prose promised: 2xx advances with no offset, 5xx redelivers, 4xx is a
    // fatal client refusal that dead-letters. The status class is the discriminant, never a per-code arm.
    public static DeliveryAck FromHttp(int status) =>
        status switch {
            >= 200 and < 300 => new Persisted(UnsetOffset),
            >= 500 => new Indeterminate($"<http:{status}>"),
            _ => new Refused(Error.New(status, $"<http-refused:{status}>")),
        };

    // The broker fault crosses WHOLE — Code, Message, inner exception, and data preserved — so HasCode/Is
    // recovery downstream reads the real broker error, never a re-minted shell that strips its identity.
    public static DeliveryAck Faulted(Error fault) => new Refused(fault);

    // The retriable/terminal partition the redrive pump reads: an Indeterminate quarantine is eligible for
    // re-delivery, a Refused quarantine is terminal evidence. Total over the closed family, never a _ arm.
    public bool Retriable => this is Indeterminate;

    public bool Advances => this is Persisted;
}

// The dead-letter evidence: the WHOLE failing CdcEnvelope (so Redrive re-projects and re-delivers it with no
// re-pull), the sink/guarantee it crossed under, the broker ack (Ack.Retriable gates redrive eligibility),
// the attempt ordinal threaded from the redelivery store (NOT a literal — the cursor counts re-pulls of the
// same entity content key), and the stamp. Redrive re-delivers the retriable rows; the OutboundHop budget
// bounds that loop. Backed by the settled Sync/coordination CoordKind.DeadLetter cell, never a second table.
public sealed record EgressDeadLetter(CdcEnvelope Entry, EgressSink Sink, DeliveryGuarantee Guarantee, DeliveryAck Ack, int Attempts, Instant At);

// One fact stream carries every egress receipt with EgressFactKind slot metadata: Sequence is the op-log
// position the cursor advanced to (per-entry, so a separate Cursor field would always equal it — collapsed),
// Offset the durably committed TopicPartitionOffset (Acked) or Unset (Delivered/Shed/Replay/DeadLetter),
// Detail the broker Error code (DeadLetter) or the re-drained count (Replay), Attempts the dead-letter
// ordinal, Elapsed the real shed duration — never five parallel buckets, never a fact thinner than the
// receipts the page promises, never a phantom slot the pump leaves unstamped.
public readonly record struct EgressFact(
    EgressFactKind Kind, string Sink, string EntityKey, long Sequence,
    TopicPartitionOffset Offset, int Attempts, string Detail, Duration Elapsed, Instant At) {
    public static EgressFact Delivered(EgressSink sink, CdcEnvelope cdc, long cursor, Instant at) =>
        new(EgressFactKind.Delivered, sink.Key, cdc.EntityKey, cursor, DeliveryAck.UnsetOffset, 0, "", Duration.Zero, at);

    public static EgressFact Acked(EgressSink sink, CdcEnvelope cdc, TopicPartitionOffset offset, long cursor, Instant at) =>
        new(EgressFactKind.Acked, sink.Key, cdc.EntityKey, cursor, offset, 0, "", Duration.Zero, at);

    public static EgressFact DeadLetter(EgressSink sink, CdcEnvelope cdc, Error fault, int attempts, long cursor, Instant at) =>
        new(EgressFactKind.DeadLetter, sink.Key, cdc.EntityKey, cursor, DeliveryAck.UnsetOffset, attempts, $"{fault.Code}:{fault.Message}", Duration.Zero, at);

    public static EgressFact Replay(EgressSink sink, long fromSequence, long redrained, Instant at) =>
        new(EgressFactKind.Replay, sink.Key, "", fromSequence, DeliveryAck.UnsetOffset, 0, $"<redrained:{redrained}>", Duration.Zero, at);

    public static EgressFact Shed(EgressSink sink, CdcEnvelope cdc, Duration elapsed, Instant at) =>
        new(EgressFactKind.Shed, sink.Key, cdc.EntityKey, cdc.Sequence, DeliveryAck.UnsetOffset, 0, "", elapsed, at);
}

// --- [SERVICES] -----------------------------------------------------------------------------------

// The registry control-plane seam the governed `WireSchema` rows share: ONE `CachedSchemaRegistryClient` per
// endpoint (never per-message, never per-topic; `ClearCaches` re-pins after an out-of-band registry mutation),
// the fixed `SubjectNameStrategy` (`Topic` for a single-type topic, `TopicRecord` for the multi-event op-log
// topic so a `BimCommitted` and a `GeometryRebaked` coexist under one topic each governed by its own subject),
// the `SchemaIdSerializerStrategy` framing (`Prefix` magic-byte default, `Header` when the id rides a
// `__value_schema_id` Kafka header beside the CloudEvents attributes), and the shared `RuleRegistry` whose
// `RuleMode.WriteRead` CSFLE `DomainRule` wraps/unwraps per-field DEKs against the `Store/encryption#KMS_PROVIDER`
// clients during serde — never a bespoke crypto pass. `Config` projects the per-row serde config disabling
// `AutoRegisterSchemas` (an incompatible producer schema is rejected at deploy via `RegisterSchemaWithResponseAsync`
// under the row's `Compatibility`) and pinning `NormalizeSchemas` so a formatting drift never churns a new id.
public sealed record RegistryGovernance(
    ISchemaRegistryClient Client,
    SubjectNameStrategy Subject,
    SchemaIdSerializerStrategy IdFraming,
    RuleRegistry Rules) {
    public AvroSerializerConfig Config(WireSchema schema) =>
        new() {
            AutoRegisterSchemas = false,
            NormalizeSchemas = true,
            SubjectNameStrategy = Subject,
            SchemaIdStrategy = IdFraming,
        };
}

// --- [OPERATIONS] ---------------------------------------------------------------------------------

public static class SerdeRail {
    // Mounts the governed serde on the `Confluent.Kafka` value slot per the sink's `WireSchema`, built ONCE per
    // stream and never per message: the `Avro` row builds an `AvroSerializer<T>` over the shared client (the
    // `Apache.Avro` `ISpecificRecord` reference codec the slot owns, the `Chr.Avro` `SchemaBuilder.BuildSchema<T>()`
    // CLR-derived expression-compiled body the OFF-Kafka leg), `Protobuf` a `ProtobufSerializer<T>` over the
    // admitted `Google.Protobuf` `IMessage<T>` wire (the `Sync/egress ← Rasm.Compute` registry-governed Protobuf
    // seam), `JsonSchema` a `JsonSerializer<T>` with server-side schema validation, and `Schemaless` mounts no
    // serde — the CloudEvents body is the value. The typed `T` constraint lives at this per-codec seam (Protobuf
    // demands `IMessage<T>`), never on the projector; CSFLE rides `governance.Rules`. Shown for the unconstrained
    // Avro leg; the Protobuf/Json mounts reparameterize the serde the row selects, never a parallel pump.
    public static ProducerBuilder<string?, T> Mount<T>(ProducerBuilder<string?, T> builder, EgressSink sink, RegistryGovernance governance) =>
        sink.Schema == WireSchema.Avro
            ? builder.SetValueSerializer(new AvroSerializer<T>(governance.Client, governance.Config(sink.Schema), governance.Rules))
            : builder;
}

public static class Egress {
    public static readonly Uri Source = new("rasm:persistence/oplog");

    public static readonly CloudEventAttribute Redacted = CloudEventAttribute.CreateExtension("redacted", CloudEventAttributeType.Boolean);

    public static readonly CloudEventAttribute Redactor = CloudEventAttribute.CreateExtension("redactor", CloudEventAttributeType.String);

    public static readonly CloudEventAttribute Classification = CloudEventAttribute.CreateExtension("classification", CloudEventAttributeType.String);

    public static readonly CloudEventAttribute Traceparent = CloudEventAttribute.CreateExtension("traceparent", CloudEventAttributeType.String);

    public static readonly CloudEventAttribute Tracestate = CloudEventAttribute.CreateExtension("tracestate", CloudEventAttributeType.String);

    // The total order rides the CNCF-standard `sequence` extension (CloudNative.CloudEvents.Extensions),
    // never a private re-declaration of the reserved attribute, so a standards-aware consumer reads it via
    // GetSequenceValue. SetSequence writes BOTH `sequence` AND `sequencetype` (the long boxes through the
    // surrogate Integer formatter), so BOTH SequenceAttribute and SequenceTypeAttribute are pre-declared here
    // — leaving sequencetype undeclared would force an implicit String auto-extension at the indexer. The
    // partitionkey/sequence/sequencetype standard extensions and the five custom attrs are the whole declared set.
    public static readonly Seq<CloudEventAttribute> Extensions =
        [Redacted, Redactor, Classification, Traceparent, Tracestate,
         Sequence.SequenceAttribute, Sequence.SequenceTypeAttribute, Partitioning.PartitionKeyAttribute];

    // The projector is sink-agnostic and codec-agnostic: one CdcEnvelope becomes one CloudEvent, and the
    // ContentMode/JsonEventFormatter bind happens at the Kafka/webhook seam (EgressDrainPort.SinkOf via
    // ToKafkaMessage), so Envelope takes neither sink nor formatter. The redacted payload rides Data as
    // ReadOnlyMemory<byte> with no defensive copy.
    public static CloudEvent Envelope(CdcEnvelope cdc) {
        var ce = new CloudEvent(Extensions) {
            Id = cdc.ContentKey.ToString("x32", CultureInfo.InvariantCulture),
            Source = Source,
            Type = $"rasm.oplog.{cdc.EntityKind}.{cdc.Kind.Key}",
            Time = cdc.At.ToDateTimeOffset(),
            DataContentType = "application/octet-stream",
            Data = cdc.Payload,
        };
        Partitioning.SetPartitionKey(ce, cdc.EntityKey);
        Sequence.SetSequence(ce, cdc.Sequence);
        ce[Redacted] = cdc.Redacted;
        // RedactorKind is the keyless [SmartEnum] (none/hmac/erase) — Map folds the case to its wire token,
        // never a phantom .Name, so a consumer renders an HMAC-pseudonymized field distinctly from an erased one.
        ce[Redactor] = cdc.Redactor.Map(none: "none", hmac: "hmac", erase: "erase");
        ce[Classification] = cdc.Classification.Key;
        if (cdc.Trace.HasParent) {
            ce[Traceparent] = Convert.ToHexString(cdc.Trace.Traceparent.Span);
            ce[Tracestate] = Encoding.ASCII.GetString(cdc.Trace.Tracestate.Span);
        }
        return ce.Validate();
    }
}
```

## [03]-[EGRESS_PUMP]

- Owner: `EgressPump` the static surface draining the op-log past a durable `SyncCursor`, projecting via `LineageCdc.Feed`, delivering through `DeliveryGuarantee.Settle`, advancing on a `Persisted` ack (`egress.acked`) or the at-most-once lossy advance (`egress.delivered`), throttling off the settled `StoreFact.BulkShed` shed fact past a one-shot shed latch, replaying from a chosen cursor, folding broker failures into the typed `EgressDeadLetter`, and re-driving the durable retriable dead-letter rows through `Redrive` so a quarantined transient failure is eventually delivered, not lost.
- Entry: `public static IO<SyncCursor> Drain(EgressSink sink, DeliveryGuarantee guarantee, SinkScope scope, SyncCursor cursor, EgressDrainPort port, ClockPolicy clocks)` — drains the `LineageCdc.Feed` redacted `CdcEnvelope` stream past the durable cursor, refines each through `scope.Admits` (the `FrozenSet` entity-kind allow-set), projects the CloudEvents event over the redacted payload, delivers it under the guarantee, and advances the cursor past each settled entry — a durable `Persisted` ack settles `egress.acked` with its committed offset, the at-most-once lossy advance settles `egress.delivered` (no durable offset), a non-`Persisted` ack settles by capture in the typed `EgressDeadLetter` so the cursor advances past the poison and `Redrive` owns the eventual re-delivery; `public static IO<SyncCursor> Replay(EgressSink sink, DeliveryGuarantee guarantee, SinkScope scope, SyncCursor from, EgressDrainPort port, ClockPolicy clocks)` re-drains the op-log prefix from a chosen cursor so a downstream re-sync replays exactly the in-scope changes (the same `Drain` fold rooted at a replayed cursor — no second drain loop), then receipts the actual re-drained span (`drained.Sequence - from.Sequence`) on `egress.replay` so the count is the real delta, never a stubbed zero; `public static IO<long> Redrive(EgressSink sink, EgressDrainPort port, ClockPolicy clocks)` re-reads the durable RETRIABLE dead-letter rows (`Ack.Retriable` gates the set so a terminal `Refused` is never re-driven), re-projects each preserved `CdcEnvelope`, re-`Settle`s under its original guarantee, and on a `Persisted` ack `Resolve`s the row to `egress.acked` while a still-non-advancing ack `Resolve`s the failed re-attempt — the `OutboundHop`'s bounded-retry budget is the loop bound so the pump never spins unbounded; back-pressure folds the `port.Shed` predicate so the FIRST slow-sink trip latches the fold, emits ONE `egress.shed` `EgressFact` carrying the REAL elapsed from the drain-start `clocks.Mark`, freezes the cursor at the last settled position, and re-pulls the unsettled tail next pass rather than spamming one shed fact per remaining entry or throwing.
- Auto: the pump reads the entries past the durable `SyncCursor` so it never re-mints a feed — it folds the pending entries threading the cursor PLUS a one-shot shed latch, filters each through the `LineageCdc.Feed` scope so a redacted or out-of-scope entry never leaves, projects the CloudEvents envelope, and delivers it through `DeliveryGuarantee.Settle`; a durable `Persisted`-acked entry advances the cursor so a crash mid-drain redelivers from the last settled position and the consumer dedups on the entity content key; a Kafka delivery folds the awaited `ProduceAsync` `DeliveryResult` into `DeliveryAck.FromResult` (`PersistenceStatus.Persisted` → `Persisted`, `NotPersisted`/`PossiblyPersisted` → `Indeterminate`) and a caught `ProduceException.Error` into `DeliveryAck.Faulted` → `Refused` carrying the whole broker `Error`, a webhook folds the HTTP status-class through `DeliveryAck.FromHttp` (2xx → `Persisted`, 5xx → `Indeterminate`, 4xx → `Refused`), and a non-`Persisted` ack captures the entry in the typed `EgressDeadLetter` carrying the attempt ordinal (`EgressDrainPort.Attempts` over the entity content key, never a literal) so the cursor advances past it and the drain continues rather than blocking — the dead-letter row is the durable settlement, `Redrive` re-delivers the retriable rows off that durable substrate, and the `OutboundHop` budget bounds the re-delivery loop, never an unbounded in-drain re-pull; `Replay` re-roots the same fold at a chosen cursor so a downstream re-sync replays the op-log prefix; back-pressure rides the existing `StoreFact.BulkShed` so the first slow-sink trip latches the fold, surfaces ONE `egress.shed` `EgressFact` carrying the real elapsed, and re-pulls the unsettled tail next pass rather than emitting a shed fact per remaining entry.
- Receipt: a durable broker ack rides `egress.acked` carrying the committed `TopicPartitionOffset` and the advanced cursor; an at-most-once fire-and-forget advance rides `egress.delivered` (no durable offset, the lossy advance) so the receipt distinguishes a confirmed write from an accepted-loss one; a dead-letter rides `egress.dead-letter` carrying the broker `Error` `Detail` and the redelivery `Attempts`; a replay rides `egress.replay` carrying the replayed-from sequence and the real re-drained count (`drained.Sequence - from.Sequence`); a shed rides `egress.shed` carrying the real `Elapsed`-at-shed measured from the drain-start `clocks.Mark`; a `Redrive` re-delivery rides `egress.acked` on success or re-stamps `egress.dead-letter` with the incremented attempt on a continued failure; every receipt is one `EgressFact` minted through its named slot factory on the page-wide stream with `EgressFactKind` slot metadata — all five slots load-bearing, none a phantom; the durable cursor is the at-least-once idempotency key, never a second cursor.
- Packages: Confluent.Kafka, NATS.Net, RabbitMQ.Client, DotPulsar, CloudNative.CloudEvents.Kafka, CloudNative.CloudEvents.SystemTextJson, Npgsql, LanguageExt.Core, NodaTime, Rasm.AppHost (project). The per-protocol publish/ack surfaces bind at the `EgressDrainPort.SinkOf` seam — the pump dials one `Func<EgressSink, CloudEvent, IO<DeliveryAck>>` and never branches on protocol.
- Growth: a new drain stance is one `DeliveryGuarantee` row whose `Settle` arm the pump already dials; a new replay shape is one entry over the cursor; a new shed policy is one predicate on `EgressDrainPort`; a new redrive disposition is one arm on the `Redrive` ack `Match`; zero new surface — a second drain loop, a parallel cursor, a per-sink replay, a per-guarantee drain method, or a parallel dead-letter re-delivery worker is the deleted form because the pump drains the one op-log past the one durable cursor, `Replay` re-roots the one fold, and `Redrive` re-pumps the one dead-letter substrate.
- Boundary: the pump drains the one op-log past the one durable `SyncCursor` — a second drain loop, a parallel cursor, or a re-minted feed is the deleted form, and `Replay` re-roots the same fold at a chosen cursor rather than minting a second drain; the cursor advances past each settled entry — a durable `Persisted` ack settles `egress.acked` directly so at-least-once redelivery from the last acked position is the normal path and the consumer dedups on the entity content key, the at-most-once lossy advance settles `egress.delivered` (the fire-and-forget receipt, no durable offset, distinct from a confirmed ack so a phantom `Delivered` slot is the deleted form), a non-`Persisted` ack settles by durable capture in the `EgressDeadLetter` so the cursor advances past the poison rather than re-pulling it forever, never a second cursor; the redaction and lineage scope are the settled `LineageCdc.Feed` projection so a masked or out-of-scope payload is what crosses, never a second redactor; the dead-letter discrimination is the closed `DeliveryAck` `Land` switch (total over `Persisted`/`Indeterminate`/`Refused`, no `_` arm — a new ack case breaks dispatch at compile time) folding the Kafka `DeliveryResult.Status` and the webhook status-class so a hardcoded sentinel fault is the deleted form and the broker `Error` (every `Refused` carries it WHOLE) or the synthesized indeterminate detail at the named `DeliveryAck.IndeterminateCode` (the ONE mint site where the broker hands back a status reason but no `Error` object) is the dead-letter evidence; both a `Refused` (fatal client/broker, terminal) and an `Indeterminate` (retriable) ack route the entry to the typed `EgressDeadLetter` so the drain continues past a poison entry and a silent drop is the deleted form, and `Redrive` re-delivers ONLY the `Ack.Retriable` rows so a transient 5xx/`PossiblyPersisted` is eventually delivered while a terminal `Refused` stays quarantined evidence — a dead-letter store that is written but never re-driven (a transient failure permanently dropped past the advanced cursor) is the deleted form; the Kafka delivery awaits `DeliveryReport` and folds `PersistenceStatus.Persisted` per the at-least-once law so a fire-and-forget produce without delivery confirmation is the rejected form on the at-least/exactly-once rows; the exactly-once row threads the Kafka producer transaction binding the CONSUMED `SyncCursor` offset (`EgressDrainPort.Txn` closes over the cursor → `SendOffsetsToTransaction`) so the produce and the cursor advance commit atomically and a partial produce is aborted; the keyed `OutboundHop` owns retry/backoff/deadline and its bounded-retry budget is the `Redrive` loop bound so the pump never re-implements the hop law nor spins unbounded (`docs/stacks/csharp/domain/resilience.md`: a durable handoff has no retry owner — persisted intent is the resilience, and `Redrive` is schedule-driven convergence over that persisted intent, not an in-drain retry loop); back-pressure rides the settled `StoreFact.BulkShed` so the first slow-sink trip latches the fold and a parallel back-pressure channel, a per-entry shed flood, or a thrown capacity exception is the deleted form; `Replay` re-drains from a chosen cursor so a downstream re-sync replays the op-log prefix, the same `SyncCursor` the `Sync/collaboration` pump acknowledges.

```csharp signature
// --- [SERVICES] -----------------------------------------------------------------------------------

// SinkOf binds the one shared JsonEventFormatter (api-cloudevents: never a per-event instance) at the
// Kafka/webhook seam via ToKafkaMessage(sink.Mode, Formatter), so the formatter is a port anchor the
// binder reads, never threaded through the projector; Attempts is the redelivery-count lookup keyed on the
// entity content key so the dead-letter ordinal is real, never a literal. Quarantined reads the durable
// retriable dead-letter rows the redrive pump re-delivers, and Resolve clears a row that finally Persisted
// or counts the failed re-attempt — so the dead-letter store is a live retry substrate, not a write-only grave.
public sealed record EgressDrainPort(
    Func<CdcScope, SyncCursor, IO<Seq<CdcEnvelope>>> Feed,
    Func<EgressSink, CloudEvent, IO<DeliveryAck>> SinkOf,
    Func<EgressSink, SyncCursor, ProducerTxn> Txn,
    Func<UInt128, IO<int>> Attempts,
    Func<EgressDeadLetter, IO<Unit>> DeadLetter,
    Func<EgressSink, IO<Seq<EgressDeadLetter>>> Quarantined,
    Func<EgressDeadLetter, DeliveryAck, IO<Unit>> Resolve,
    Func<EgressFact, IO<Unit>> Fact,
    Func<SyncCursor, bool> Shed,
    JsonEventFormatter Formatter);

// --- [OPERATIONS] ---------------------------------------------------------------------------------

public static class EgressPump {
    // The drain accumulator threads the advanced cursor PLUS the one-shot shed latch: once the slow-sink
    // predicate trips, the latch holds and every later entry passes through untouched (one shed fact, not one
    // per entry), the cursor frozen at the last settled position so the unsettled tail re-pulls next pass.
    // The shed fact carries the REAL elapsed from the drain-start mark, never a stubbed Duration.Zero.
    public static IO<SyncCursor> Drain(
        EgressSink sink, DeliveryGuarantee guarantee, SinkScope scope, SyncCursor cursor,
        EgressDrainPort port, ClockPolicy clocks) =>
        IO.lift(clocks.Mark).Bind(mark =>
            port.Feed(scope.Lineage, cursor).Map(stream => stream.Filter(scope.Admits)).Bind(entries =>
                entries.Fold(
                    IO.pure((Cursor: cursor, Shed: false)),
                    (acc, cdc) => acc.Bind(state => state.Shed
                        ? IO.pure(state)
                        : port.Shed(state.Cursor)
                            ? port.Fact(EgressFact.Shed(sink, cdc, clocks.Elapsed(mark), clocks.Now)).Map(_ => (Cursor: state.Cursor, Shed: true))
                            : guarantee.Settle(port.SinkOf(sink, Egress.Envelope(cdc)), port.Txn(sink, state.Cursor))
                                .Bind(ack => Land(sink, guarantee, cdc, ack, state.Cursor, port, clocks)).Map(advanced => (Cursor: advanced, Shed: false))))))
            .Map(static state => state.Cursor);

    // Replay re-roots the one Drain fold at a chosen cursor and receipts the actual re-drained span (the
    // delta between the from-sequence and the cursor Drain returns), so egress.replay carries a real count.
    public static IO<SyncCursor> Replay(
        EgressSink sink, DeliveryGuarantee guarantee, SinkScope scope, SyncCursor from,
        EgressDrainPort port, ClockPolicy clocks) =>
        Drain(sink, guarantee, scope, from, port, clocks).Bind(drained =>
            port.Fact(EgressFact.Replay(sink, from.Sequence, drained.Sequence - from.Sequence, clocks.Now)).Map(_ => drained));

    // Redrive re-delivers the durable RETRIABLE dead-letter rows the drain quarantined past the cursor — the
    // leg that makes the dead-letter store a live retry substrate rather than a write-only grave, so an
    // Indeterminate (transient broker hiccup, 5xx) is eventually delivered, not silently lost. Each row
    // re-projects its preserved CdcEnvelope, re-Settles under its original guarantee, and on a Persisted ack
    // Resolve clears the row (egress.acked); a still-non-advancing ack Resolves the failed re-attempt so the
    // OutboundHop's bounded-retry budget is the loop bound, never an unbounded in-pump spin. A terminal Refused
    // row is never re-driven — Ack.Retriable gates the set at the source.
    public static IO<long> Redrive(EgressSink sink, EgressDrainPort port, ClockPolicy clocks) =>
        port.Quarantined(sink).Bind(rows => rows.Filter(static row => row.Ack.Retriable).Fold(
            IO.pure(0L),
            (acc, row) => acc.Bind(count =>
                row.Guarantee.Settle(port.SinkOf(sink, Egress.Envelope(row.Entry)), ProducerTxn.None)
                    .Bind(ack => port.Resolve(row, ack).Bind(_ => ack.Match(
                        Persisted: persisted => port.Fact(EgressFact.Acked(sink, row.Entry, persisted.Offset, row.Entry.Sequence, clocks.Now)).Map(_ => count + 1L),
                        Indeterminate: indeterminate => port.Fact(EgressFact.DeadLetter(sink, row.Entry, Error.New(DeliveryAck.IndeterminateCode, indeterminate.Reason), row.Attempts + 1, row.Entry.Sequence, clocks.Now)).Map(_ => count),
                        Refused: refused => port.Fact(EgressFact.DeadLetter(sink, row.Entry, refused.Fault, row.Attempts + 1, row.Entry.Sequence, clocks.Now)).Map(_ => count)))))));

    // A durable Persisted ack settles egress.acked with its committed offset; the at-most-once lossy advance
    // (UnsetOffset, no durable confirmation) settles egress.delivered so the fire-and-forget receipt is
    // honest and distinct; every non-advancing ack settles by durable capture in the EgressDeadLetter carrying
    // the attempt ordinal the redelivery store counts (NEVER a literal), so the cursor advances past the poison
    // and Redrive owns the eventual re-delivery off that row. The generated [Union] Match is compile-time
    // exhaustive — a new DeliveryAck case breaks this dispatch, never a runtime-silent _ arm.
    private static IO<SyncCursor> Land(
        EgressSink sink, DeliveryGuarantee guarantee, CdcEnvelope cdc, DeliveryAck ack,
        SyncCursor advanced, EgressDrainPort port, ClockPolicy clocks) =>
        ack.Match(
            Persisted: persisted => (guarantee == DeliveryGuarantee.AtMostOnce
                    ? port.Fact(EgressFact.Delivered(sink, cdc, cdc.Sequence, clocks.Now))
                    : port.Fact(EgressFact.Acked(sink, cdc, persisted.Offset, cdc.Sequence, clocks.Now)))
                .Map(_ => advanced with { Sequence = cdc.Sequence }),
            Indeterminate: indeterminate => Quarantine(sink, guarantee, cdc, ack, Error.New(DeliveryAck.IndeterminateCode, indeterminate.Reason), advanced, port, clocks),
            Refused: refused => Quarantine(sink, guarantee, cdc, ack, refused.Fault, advanced, port, clocks));

    private static IO<SyncCursor> Quarantine(
        EgressSink sink, DeliveryGuarantee guarantee, CdcEnvelope cdc, DeliveryAck ack, Error fault,
        SyncCursor advanced, EgressDrainPort port, ClockPolicy clocks) =>
        port.Attempts(cdc.ContentKey).Bind(attempts =>
            port.DeadLetter(new EgressDeadLetter(cdc, sink, guarantee, ack, attempts, clocks.Now))
                .Bind(_ => port.Fact(EgressFact.DeadLetter(sink, cdc, fault, attempts, cdc.Sequence, clocks.Now)))
                .Map(_ => advanced with { Sequence = cdc.Sequence }));
}
```

| [INDEX] | [POLICY]              | [VALUE]                                          | [BINDING]                                                          |
| :-----: | :-------------------- | :----------------------------------------------- | :----------------------------------------------------------------- |
|  [01]   | feed source           | the one op-log past `SyncCursor`                 | `LineageCdc.Feed` filters; never a second changefeed              |
|  [02]   | envelope              | CloudEvents v1.0 over redacted `CdcEnvelope`     | `JsonEventFormatter` + `ToKafkaMessage(sink.Mode)`; `redactor`/`classification` extension attrs |
|  [03]   | delivery ceremony     | `DeliveryGuarantee.Settle` carrier-dispatch      | fire-and-forget / await-ack / producer-txn; never a guarantee flag |
|  [04]   | cursor advance        | durable `Persisted` → `egress.acked`; lossy → `egress.delivered` | `DeliveryResult.Status == Persisted` advances at-least/exactly-once; at-most-once advances on the lossy ack; redelivery is normal |
|  [05]   | exactly-once          | produce + cursor advance in one Kafka txn        | `ProducerTxn.Seal` binds the CONSUMED `SyncCursor` via `SendOffsetsToTransaction`/`CommitTransaction` on `Persisted`, `AbortTransaction` on every non-advancing ack |
|  [06]   | dead-letter           | `DeliveryAck` `Land` switch on `Indeterminate`/`Refused` | `FromResult`/`FromHttp` fold; whole broker `Error` in `Detail` (or `DeliveryAck.IndeterminateCode`), `Attempts` from the redelivery store; drain continues past poison |
|  [07]   | redrive               | `Redrive` re-delivers `Ack.Retriable` dead-letter rows | re-`Settle` over the preserved `CdcEnvelope`; `OutboundHop` budget bounds the loop; `Refused` rows stay terminal evidence |
|  [08]   | hop ownership         | keyed AppHost `OutboundHop` per webhook/gRPC      | retry/backoff/deadline owned at AppHost; database outside hop law |
|  [09]   | partition key         | `Partitioning.SetPartitionKey` from entity key   | co-keyed changes preserve per-key ordering on one partition      |
|  [10]   | back-pressure         | latched `StoreFact.BulkShed` shed fact off the stream | first slow-sink trip latches the fold, ONE shed fact carries the real `clocks.Mark` elapsed; never a per-entry flood or a thrown capacity exception |
|  [11]   | replay                | `Replay` re-roots the one `Drain` fold           | one entry over the cursor; never a second drain loop             |

## [04]-[RESEARCH]

- [KAFKA_DELIVERY_ACK]: the `Confluent.Kafka` `ProduceAsync` → `DeliveryResult.Status == Persisted` at-least-once confirmation (success returns a `DeliveryResult`, failure throws `ProduceException<string?, byte[]>` carrying `.Error` and `.DeliveryResult`), the `ProduceException.Error` → `DeliveryAck.Faulted` → `Refused` routing that crosses the whole broker `Error` (Code, Message, inner exception preserved — never re-minted), and the `EnableIdempotence`/transactional exactly-once path (`InitTransactions`/`BeginTransaction`/`SendOffsetsToTransaction`/`CommitTransaction`/`AbortTransaction` threaded by `ProducerTxn.Seal` off the settled ack against the consumed `IConsumerGroupMetadata`) against a live broker — whether the cursor advances only on the durable `Persisted` ack, a `NotPersisted`/`PossiblyPersisted` folds to `Indeterminate` and redelivers, and a `ProduceException.Error.IsFatal` folds to `Refused` and routes to the dead-letter, proven before the delivery fence pins (`Error.IsRetriable` is `internal` in `Confluent.Kafka` and is never read — the public `IsFatal`/`IsBrokerError`/`Code` carry the discrimination).
- [CLOUDEVENTS_BINDING]: the `cloudEvent.ToKafkaMessage(ContentMode.Binary, JsonEventFormatter)` Kafka binding and the `Partitioning.SetPartitionKey` partition-key routing — whether the redacted `CdcEnvelope` payload and the `traceparent`/`tracestate`/`redacted`/`redactor`/`classification`/`sequence` extension attributes cross in binary mode so a broker filters on headers and the Python runtime-transport leg decodes the CloudEvents event, with `CloudEvent.Validate()` rejecting an incomplete envelope at projection, confirmed against the admitted `CloudNative.CloudEvents.Kafka`.
- [OUTBOUNDHOP_REGISTRATION]: the keyed AppHost `OutboundHop` registration for a webhook/gRPC egress — the retry/backoff/deadline the hop owner carries, the bounded-retry budget that bounds the `Redrive` loop, and the `[ONE_OUTBOX_EGRESS_SPINE]` binding of this egress, the AppHost outbox relay, and the durable-orchestration dispatch as keyed hop consumers over the one CloudEvents envelope, confirmed against the `Rasm.AppHost/Runtime` hop surface before the seam pins.
- [DEAD_LETTER_REDRIVE]: the durable dead-letter store contract behind `EgressDrainPort.Quarantined`/`Resolve`/`DeadLetter` and the `Redrive` re-delivery fold — whether the store persists the whole `EgressDeadLetter` (the preserved `CdcEnvelope` so re-projection needs no re-pull), filters `Ack.Retriable` so a terminal `Refused` is never re-driven, increments the `Attempts` ordinal the `EgressDrainPort.Attempts` lookup reads off the entity content key, and resolves a row on a `Persisted` re-delivery — proving the cursor-advances-past-poison disposition pairs with an eventual-delivery leg so a transient `Indeterminate` (a 5xx/`PossiblyPersisted`) is delivered, never silently dropped, the redrive cadence being schedule-driven convergence over persisted intent (`resilience.md`: a durable handoff has no retry owner) rather than an in-drain retry loop, confirmed against the `Sync/coordination` outbox/dead-letter `CoordKind` store this dead-letter row composes.

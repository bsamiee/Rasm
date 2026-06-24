# [PERSISTENCE_EGRESS]

Rasm.Persistence fans the one op-log changefeed to external sinks with at-least-once delivery, dead-letter capture, and cursor replay — without minting a second changefeed. `EgressSink` is the `[SmartEnum<string>]` sink axis (webhook, NATS, Kafka, RabbitMQ, Arrow-Flight-push, gRPC-stream) crossed with `DeliveryGuarantee` and `SinkScope`; `EgressPump` drains the settled `Version/provenance#LINEAGE_CDC` `LineageCdc.Feed` redaction-aware `CdcEnvelope` stream past a durable `SyncCursor`, wraps each redacted envelope in a CloudEvents v1.0 event carrying the `CdcEnvelope.Payload` in `Data`, delivers it, advances the cursor past each settled entry, and routes a non-acked entry to a typed `EgressDeadLetter`. The admitted `Confluent.Kafka` carries broker delivery and the `CloudNative.CloudEvents`/`.Kafka`/`.SystemTextJson` carry the standard envelope and the Kafka binding; `LineageCdc.Feed`/`CdcEnvelope`/`CdcScope`, `OpLog`/`SyncCursor`/`OpLogEntryWire`, `ArrowEgress`/`ArrowChunk`, the AppHost `OutboundHop`, `ClockPolicy`, and `ReceiptSinkPort` arrive settled. Each webhook/gRPC egress registers as a keyed AppHost `OutboundHop` through the `Sync/egress ⇄ Rasm.AppHost/Runtime # [PORT]: keyed OutboundHop egress` seam — one of three keyed-hop consumers the AppHost `[ONE_OUTBOX_EGRESS_SPINE]` branch binds over the one op-log (this egress, the outbox relay, and the durable-orchestration step dispatch) draining the same `CdcEnvelopeWire` CloudEvents envelope; the Python runtime-transport leg decodes the CloudEvents event whose `Data` is the redacted op payload.

Wire posture: this page projects the redacted CDC stream onto the CloudEvents wire — the event carries the `CdcEnvelope`-projected payload and the trace context, so the egress consumers (`python:runtime/transport`, external brokers) decode the one CloudEvents envelope, never a re-minted shape; the redaction is the settled `LineageCdc.Feed`/`ExportProof` masking, so an out-of-authority payload crosses masked rather than raw.

## [01]-[INDEX]

- [01]-[EGRESS_SINK]: the sink axis, delivery guarantee, scope, and the CloudEvents envelope.
- [02]-[EGRESS_PUMP]: the op-log-drain fold past a durable cursor, the durable-ack advance, and the dead-letter.

## [02]-[EGRESS_SINK]

- Owner: `EgressSink` the `[SmartEnum<string>]` sink axis under the `SyncKeyPolicy` ordinal accessor; `DeliveryGuarantee` the `[SmartEnum<string>]` delivery-semantics axis; `SinkScope` the lineage-scope-and-entity-kind filter record over the settled `CdcScope`; `Egress.Envelope` the CloudEvents projection of a redacted `CdcEnvelope`; `EgressDeadLetter` the typed dead-letter record; `EgressFact` with `EgressFactKind` the page-wide fact stream.
- Cases: `EgressSink` webhook | nats | kafka | rabbitmq | arrow-flight-push | grpc-stream; `DeliveryGuarantee` at-least-once | at-most-once | exactly-once; `SinkScope` carries the `CdcScope` lineage filter the `LineageCdc.Feed` applies plus the entity-kind allow-set the pump refines on; `EgressFactKind` delivered | acked | dead-letter | replay | shed.
- Entry: `public static CloudEvent Envelope(CdcEnvelope cdc, EgressSink sink, JsonEventFormatter formatter)` projects a redaction-aware `CdcEnvelope` into a CloudEvents v1.0 event carrying the redacted `cdc.Payload` in `Data`, the `redacted` flag and the `traceparent` from the envelope's `TraceContext` as extension attributes, and the partition key from the entity key; `public static IO<bool> Deliver(EgressSink sink, CloudEvent envelope, DeliveryGuarantee guarantee, Func<CloudEvent, IO<bool>> sinkOf)` delivers the event under the guarantee ceremony and returns the durable-ack result.
- Auto: the egress reuses the one op-log changefeed through `LineageCdc.Feed` — the feed already filters by lineage scope and projects the redaction-aware payload but pushes nowhere, so this owner is purely the drain-and-push leg over its `CdcEnvelope` stream; each envelope projects into a CloudEvents v1.0 event with required `Id`/`Source`/`Type`/`Time`, the redacted `cdc.Payload` in `Data`, the `redacted` and `traceparent` extension attributes so a downstream consumer extract-and-continues the originating span (realizing `ONE_DISTRIBUTED_TRACE` across the egress) and reads whether the payload was masked, and the partition key set through `Partitioning.SetPartitionKey` from the entity key so co-keyed changes preserve per-key ordering on one partition; the Kafka sink composes `cloudEvent.ToKafkaMessage(ContentMode.Binary, formatter)` so attributes stay in headers and a broker filters without parsing the body, the webhook/gRPC sinks register as keyed AppHost `OutboundHop` rows so retry/backoff/deadline are the hop owner's concern, the Arrow-Flight-push sink rides the settled `Query/rail#ARROW_EGRESS` Flight stream for a columnar bulk egress, and the NATS/RabbitMQ sinks ride their broker bindings; `DeliveryGuarantee.AtLeastOnce` is the default — deliver, await the durable ack (`DeliveryResult.Status == Persisted` on Kafka, the 2xx on webhook), advance the cursor only on the ack so redelivery after crash is the normal path; `DeliveryGuarantee.ExactlyOnce` pairs the Kafka `InitTransactions`/`BeginTransaction`/`CommitTransaction` against the consumed cursor offset; `DeliveryGuarantee.AtMostOnce` fires without awaiting the durable ack; `SinkScope` carries the settled `LineageCdc.Feed` lineage scope so a sink's redaction is the settled filter, never a second redactor.
- Receipt: a delivery rides `egress.delivered` carrying the sink and the entity key; a durable ack rides `egress.acked` carrying the advanced cursor; a dead-letter rides `egress.dead-letter` carrying the failing entry and the broker error; a replay rides `egress.replay`; the typed `EgressDeadLetter(Entry, Sink, Error, Attempts, Instant)` carries the dead-letter evidence; an at-least-once delivery without a durable ack is a typed egress fault, never silent.
- Packages: Confluent.Kafka, CloudNative.CloudEvents, CloudNative.CloudEvents.Kafka, CloudNative.CloudEvents.SystemTextJson, Apache.Arrow.Flight, System.IO.Hashing, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Rasm.AppHost (project).
- Growth: a new sink is one `EgressSink` row plus its binding; a new delivery semantic is one `DeliveryGuarantee` row; a new scope filter is one column on `SinkScope`; a new evidence bucket is one `EgressFactKind` row; zero new surface — a second changefeed, a parallel envelope shape, or a per-sink CDC feed is the deleted form because the egress drains the one op-log and projects the one CloudEvents envelope.
- Boundary: the egress drains the one op-log changefeed through `LineageCdc.Feed` and never mints a second feed — the feed already filters by lineage scope and projects the redacted payload, and this owner adds only the drain-and-push leg, so a parallel CDC feed or a trigger-based second changefeed is the deleted form; the wire payload is the redacted `CdcEnvelope.Payload` so an out-of-authority change crosses masked rather than raw — projecting the raw `OpLogEntry.Payload` past the redaction is the deleted form; the envelope is the one CloudEvents v1.0 projection — a hand-rolled CloudEvents JSON layout or a manual `ce_` Kafka header is the rejected form, the `JsonEventFormatter` encodes and `ToKafkaMessage(ContentMode.Binary)` binds; the webhook/gRPC sinks register as keyed AppHost `OutboundHop` rows through the `Sync/egress ⇄ Rasm.AppHost/Runtime # [PORT]: keyed OutboundHop egress` seam so retry/backoff/deadline/correlation are the hop owner's concern and a second retry surface is the deleted form; `DeliveryGuarantee.AtLeastOnce` advances the cursor only on durable ack so redelivery after crash is the normal path and the consumer dedups on the entity content key; `Confluent.Kafka` is heavy and droppable to webhook/gRPC/Flight while retaining the CloudEvents envelope if broker scope is rejected; `SinkScope` carries the settled `LineageCdc.Feed` lineage scope and the `Version/retention#EXPORT` `ExportProof` redactor so a sink's masking is the settled filter, never a second redactor; the AppHost `[ONE_OUTBOX_EGRESS_SPINE]` branch binds three keyed `OutboundHop` consumers over the one op-log — this Persistence egress, the AppHost `Wire/outbox` relay, and the AppHost `Runtime/orchestration` durable-orchestration step dispatch — each draining the same `CdcEnvelopeWire` CloudEvents envelope as the hop payload, so a second outbox owner, a relay-side re-pack, or a per-consumer changefeed is the drift defect and the CloudEvents projection is the one wire payload across all three consumers; the Python runtime-transport leg decodes the CloudEvents event whose `Data` is the redacted op payload so the cross-language consumer reads the one envelope vocabulary.

```csharp signature
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
    public static readonly EgressSink Webhook = new("webhook", broker: false);
    public static readonly EgressSink Nats = new("nats", broker: true);
    public static readonly EgressSink Kafka = new("kafka", broker: true);
    public static readonly EgressSink RabbitMq = new("rabbitmq", broker: true);
    public static readonly EgressSink ArrowFlightPush = new("arrow-flight-push", broker: false);
    public static readonly EgressSink GrpcStream = new("grpc-stream", broker: false);

    public bool Broker { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<SyncKeyPolicy, string>]
[KeyMemberComparer<SyncKeyPolicy, string>]
public sealed partial class DeliveryGuarantee {
    public static readonly DeliveryGuarantee AtLeastOnce = new("at-least-once");
    public static readonly DeliveryGuarantee AtMostOnce = new("at-most-once");
    public static readonly DeliveryGuarantee ExactlyOnce = new("exactly-once");
}

public sealed record SinkScope(CdcScope Lineage, Seq<string> EntityKinds) {
    public bool Admits(CdcEnvelope cdc) => EntityKinds.IsEmpty || EntityKinds.Contains(cdc.EntityKind);
}

public sealed record EgressDeadLetter(CdcEnvelope Entry, EgressSink Sink, Error Fault, int Attempts, Instant At);

public readonly record struct EgressFact(EgressFactKind Kind, string Sink, string EntityKey, long Sequence, Duration Elapsed, Instant At);

public static class Egress {
    public static readonly Uri Source = new("rasm:persistence/oplog");

    public static readonly CloudEventAttribute Sequence = CloudEventAttribute.CreateExtension("sequence", CloudEventAttributeType.Integer);

    public static readonly CloudEventAttribute Redacted = CloudEventAttribute.CreateExtension("redacted", CloudEventAttributeType.Boolean);

    public static CloudEvent Envelope(CdcEnvelope cdc, EgressSink sink, JsonEventFormatter formatter) {
        var ce = new CloudEvent {
            Id = cdc.ContentKey.ToString("x32", CultureInfo.InvariantCulture),
            Source = Source,
            Type = $"rasm.oplog.{cdc.EntityKind}.{cdc.Kind.Key}",
            Time = cdc.At.ToDateTimeOffset(),
            DataContentType = "application/octet-stream",
            Data = cdc.Payload.ToArray(),
        };
        Partitioning.SetPartitionKey(ce, cdc.EntityKey);
        ce[Sequence] = cdc.Sequence;
        ce[Redacted] = cdc.Redacted;
        if (cdc.Trace.HasParent)
            ce.SetAttributeFromString("traceparent", Convert.ToHexString(cdc.Trace.Traceparent.Span));
        return ce;
    }

    public static IO<bool> Deliver(EgressSink sink, CloudEvent envelope, DeliveryGuarantee guarantee, Func<CloudEvent, IO<bool>> sinkOf) =>
        guarantee.Switch(
            state: (Sink: sink, Envelope: envelope, SinkOf: sinkOf),
            atLeastOnce: static s => s.SinkOf(s.Envelope),
            exactlyOnce: static s => s.SinkOf(s.Envelope),
            atMostOnce: static s => s.SinkOf(s.Envelope).Map(static _ => true));
}
```

## [03]-[EGRESS_PUMP]

- Owner: `EgressPump` the static surface draining the op-log past a durable `SyncCursor`, projecting via `LineageCdc.Feed`, delivering to the sink, advancing on durable ack, and routing failures to the typed `EgressDeadLetter`.
- Entry: `public static IO<SyncCursor> Drain(EgressSink sink, SinkScope scope, SyncCursor cursor, Func<CdcScope, SyncCursor, IO<Seq<CdcEnvelope>>> feed, Func<CloudEvent, IO<bool>> sinkOf, JsonEventFormatter formatter, Func<EgressDeadLetter, IO<Unit>> deadLetter, ClockPolicy clocks)` — drains the `LineageCdc.Feed` redacted `CdcEnvelope` stream past the durable cursor, refines each through `scope.Admits` (the entity-kind allow-set), projects the CloudEvents event over the redacted payload, delivers it, and advances the cursor past each settled entry — a durable ack settles the entry directly, a non-acked entry settles by capture in the typed `EgressDeadLetter` so the durable dead-letter row replaces re-pull and the drain never poison-loops on one entry.
- Auto: the pump reads the entries past the durable `SyncCursor` so it never re-mints a feed — it folds the pending entries, filters each through the `LineageCdc.Feed` scope so a redacted or out-of-scope entry never leaves, projects the CloudEvents envelope, and delivers it; a durably-acked entry advances the cursor so a crash mid-drain redelivers from the last settled position and the consumer dedups on the entity content key; a Kafka delivery awaits `DeliveryResult.Status == Persisted`, a webhook awaits the 2xx, and a non-acked entry (`NotPersisted`/`PossiblyPersisted`, a `DeliveryReport.Error.IsFatal`, a 4xx) is captured in the typed `EgressDeadLetter` and the cursor advances past it so the drain continues rather than blocking — the dead-letter row is the durable settlement and the `OutboundHop` owns the bounded retry off that row, never an unbounded in-drain re-pull; cursor replay re-drains from a chosen cursor so a downstream re-sync replays the op-log prefix; back-pressure rides the existing `BulkShed` so a slow sink throttles off the existing fact stream.
- Receipt: a drain rides `egress.delivered`/`egress.acked` carrying the advanced cursor; a dead-letter rides `egress.dead-letter`; a replay rides `egress.replay`; the durable cursor is the at-least-once idempotency key, never a second cursor.
- Packages: Confluent.Kafka, CloudNative.CloudEvents.Kafka, CloudNative.CloudEvents.SystemTextJson, Npgsql, LanguageExt.Core, NodaTime, Rasm.AppHost (project).
- Growth: a new drain stance is one policy value on the pump; a new replay shape is one entry over the cursor; zero new surface — a second drain loop, a parallel cursor, or a per-sink replay is the deleted form because the pump drains the one op-log past the one durable cursor.
- Boundary: the pump drains the one op-log past the one durable `SyncCursor` — a second drain loop, a parallel cursor, or a re-minted feed is the deleted form; the cursor advances past each settled entry — a durable ack settles directly so at-least-once redelivery from the last acked position is the normal path and the consumer dedups on the entity content key, a non-acked entry settles by durable capture in the `EgressDeadLetter` so the cursor advances past the poison rather than re-pulling it forever, never a second cursor; the redaction and lineage scope are the settled `LineageCdc.Feed` projection so a masked or out-of-scope payload is what crosses, never a second redactor; a non-retriable failure routes the entry to the typed `EgressDeadLetter` so the drain continues past a poison entry and a silent drop is the deleted form; the Kafka delivery awaits `DeliveryResult.Status == Persisted` per the at-least-once law so a fire-and-forget produce without delivery confirmation is the rejected form; the keyed `OutboundHop` owns retry/backoff/deadline so the pump never re-implements the hop law; cursor replay re-drains from a chosen cursor so a downstream re-sync replays the op-log prefix, the same `SyncCursor` the `Sync/collaboration` pump acknowledges.

```csharp signature
public static class EgressPump {
    public static IO<SyncCursor> Drain(
        EgressSink sink, SinkScope scope, SyncCursor cursor,
        Func<CdcScope, SyncCursor, IO<Seq<CdcEnvelope>>> feed,
        Func<CloudEvent, IO<bool>> sinkOf,
        JsonEventFormatter formatter,
        Func<EgressDeadLetter, IO<Unit>> deadLetter,
        ClockPolicy clocks) =>
        feed(scope.Lineage, cursor).Map(stream => stream.Filter(scope.Admits)).Bind(entries =>
            entries.Fold(
                IO.pure(cursor),
                (acc, cdc) => acc.Bind(advanced =>
                    Egress.Deliver(sink, Egress.Envelope(cdc, sink, formatter), DeliveryGuarantee.AtLeastOnce, sinkOf)
                        .Bind(acked => acked
                            ? IO.pure(advanced with { Sequence = cdc.Sequence })
                            : deadLetter(new EgressDeadLetter(cdc, sink, Error.New("<egress-not-acked>"), 1, clocks.Now))
                                .Map(_ => advanced with { Sequence = cdc.Sequence })))));
}
```

| [INDEX] | [POLICY]              | [VALUE]                                          | [BINDING]                                                          |
| :-----: | :-------------------- | :----------------------------------------------- | :----------------------------------------------------------------- |
|  [01]   | feed source           | the one op-log past `SyncCursor`                 | `LineageCdc.Feed` filters; never a second changefeed              |
|  [02]   | envelope              | CloudEvents v1.0 over redacted `CdcEnvelope`     | `JsonEventFormatter` + `ToKafkaMessage(Binary)`                   |
|  [03]   | delivery guarantee    | at-least-once: advance cursor on durable ack     | `DeliveryResult.Status == Persisted`; redelivery is normal       |
|  [04]   | dead-letter           | typed `EgressDeadLetter` on non-retriable error  | `DeliveryReport.Error.IsFatal`; drain continues past poison      |
|  [05]   | hop ownership         | keyed AppHost `OutboundHop` per webhook/gRPC      | retry/backoff/deadline owned at AppHost; database outside hop law |
|  [06]   | partition key         | `Partitioning.SetPartitionKey` from entity key   | co-keyed changes preserve per-key ordering on one partition      |

## [04]-[RESEARCH]

- [KAFKA_DELIVERY_ACK]: the `Confluent.Kafka` `ProduceAsync` → `DeliveryResult.Status == Persisted` at-least-once confirmation and the `EnableIdempotence`/transactional exactly-once path against a live broker — whether the cursor advances only on the durable ack and a `DeliveryReport.Error.IsFatal` routes to the dead-letter, proven before the delivery fence pins.
- [CLOUDEVENTS_BINDING]: the `cloudEvent.ToKafkaMessage(ContentMode.Binary, JsonEventFormatter)` Kafka binding and the `Partitioning.SetPartitionKey` partition-key routing — whether the redacted `CdcEnvelope` payload and the `traceparent`/`redacted` extension attributes cross in binary mode so a broker filters on headers and the Python runtime-transport leg decodes the CloudEvents event, confirmed against the admitted `CloudNative.CloudEvents.Kafka`.
- [OUTBOUNDHOP_REGISTRATION]: the keyed AppHost `OutboundHop` registration for a webhook/gRPC egress — the retry/backoff/deadline the hop owner carries and the `[ONE_OUTBOX_EGRESS_SPINE]` binding of this egress, the AppHost outbox relay, and the durable-orchestration dispatch as keyed hop consumers over the one CloudEvents envelope, confirmed against the `Rasm.AppHost/Runtime` hop surface before the seam pins.

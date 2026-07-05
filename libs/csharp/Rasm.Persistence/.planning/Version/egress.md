# [PERSISTENCE_VERSION_EGRESS]

Rasm.Persistence owns the CDC egress beside the changefeed it drains: ONE `EgressPump` fold draining the `Store/coordination#OUTBOX_CURSOR` per-sink cursor past `Version/ledger#CHANGEFEED` `OpLogEntry` rows into `EgressSink` delivery rows under ONE CloudEvents envelope — exactly-once EFFECT, spelled as content-addressed identity plus per-sink dedup composition, never a broker guarantee: `id` = `OpLogEntry.ContentKey`, `Sequence` = the outbox cursor `long`, `Partitioning.partitionkey` = `EntityKey`, so every sink that can dedup on a producer-supplied id dedups on the content key and every sink that cannot carries at-least-once with receiver-side id-dedup as its honest stance. The sinks are seed DATA on one closed `[Union]` — webhook (`pg_net`, advance ONLY on `net.request_status = 'SUCCESS'` reconciliation: PENDING holds the cursor, ERROR/timeout dead-letters, and an UNLOGGED-table crash re-posts under the idempotency-key header), NATS (`id` → `Nats-Msg-Id`, JetStream `PubAckResponse` Settle-ack advances, `Duplicate` is the dedup-window replay absorbed), Kafka (awaited `ProduceAsync` → `DeliveryResult.Status == Persisted`; the producer transaction `InitTransactions`/`SendOffsetsToTransaction`/`CommitTransaction` is the optional `Seal` row committing batch plus cursor atomically — the one sink whose exactly-once is real, composed as policy), RabbitMQ (v7 `BasicPublishAsync` publisher-confirms), Pulsar (`ISend.Send` → `MessageId`), wire-native gRPC/Arrow (Persistence supplies length-prefixed bytes; the channel and delivery-honesty ride the AppHost `Wire/outbound` `OutboundHop` policy — the keyed egress counterpart), and redis-stream (`XADD` via `IDatabase.StreamAdd` keyed `StreamIdempotentId` = content key for producer-side at-most-once, consumer-group `StreamReadGroup`/`StreamAcknowledge` ack advancing the per-sink cursor, `StreamTrimMode.Acknowledged` the retention stance — the zero-broker-install stream row). Every sink outcome folds to ONE `DeliveryAck` `[Union]` (`Persisted`/`Indeterminate`/`Refused`) and only `Persisted` advances the cursor. The pump drains DURABLE lanes ONLY — presence/awareness stays the `ledger#PRESENCE` lossy `AwarenessLane` `DrainSurface` and NEVER enters the exactly-once CDC envelope; classified payload fields `Redact` before the envelope leaves the trust boundary. Wall clock, correlation, and tenant ride the injected `Element/graph#STORE_RAIL` `ProjectionContext` frame — a `ClockPolicy`/`CorrelationId` parameter on any signature here is the named strata inversion; `FaultBand` arrives from `Element/graph#FAULT_TABLES`, `OpLogEntry`/`ReplayWindow` from `Version/ledger`, `OutboxCursor`/`OutboxAdvance` from `Store/coordination`, `IValidityEvidence`/`ValidityClaim` from the `Rasm` kernel.

## [01]-[INDEX]

- [01]-[EGRESS_PUMP]: the one drain fold over the per-sink cursor, the advance law, the typed dead-letter and replay rows, the `EgressReceipt` validity fold, and the 8270 fault band.
- [02]-[EGRESS_SINK]: the `CdcEnvelope` CloudEvents projection, the closed `EgressSink` delivery-row family, the `DeliveryAck` fold, and the per-sink dedup honesty table.

## [02]-[EGRESS_PUMP]

- Owner: `EgressPump` the static surface owning the one drain bracket — cursor read, windowed row drain, envelope projection, sink delivery, ack fold, cursor advance, dead-letter capture; `DeadLetterRow` the typed dead-letter document (content key, sink, sequence, fault, attempt count) stored in the SAME Marten session so a dead-letter and its cursor state commit atomically; `EgressReceipt` the per-drain evidence implementing the kernel `IValidityEvidence`; `EgressFault` the 8270 band; `EgressPorts` the injected delegate frame (the sink clients, the `Coordinate.Run` advance arrow, the redactor) filled at the composition root.
- Entry: `public static IO<Fin<EgressReceipt>> Drain(EgressSink sink, OutboxCursor cursor, EgressPorts ports, ProjectionContext frame)` is the one pump — it drains `ReplayWindow.DurableOps(cursor.Sequence, sink.Batch)` rows from the changefeed (the `Version/ledger` windowed-read case parameterized for this drain — never a third read surface), projects each row through `Egress.Envelope`, delivers through the sink row's composition, folds every outcome to `DeliveryAck`, and advances the cursor through `Store/coordination` `OutboxAdvance(sink.Key, through)` ONLY past the contiguous `Persisted` prefix — the first `Indeterminate` holds the cursor at its predecessor (a held cursor re-drains, the sink's dedup absorbs the replay), a `Refused` writes the `DeadLetterRow` and the drain continues past it; `public static IO<Fin<EgressReceipt>> Replay(EgressSink sink, Seq<DeadLetterRow> letters, EgressPorts ports, ProjectionContext frame)` re-delivers dead-lettered entries by content key through the SAME envelope/deliver/ack fold — replay is the pump re-parameterized, never a second delivery path.
- Auto: the drain is one fold per batch — `rows.Map(Egress.Envelope).Map(sink.Deliver)` folded left with the contiguous-prefix advance accumulator, so ordering per partition key is preserved (the envelope's `partitionkey` = `EntityKey` keeps per-entity order inside every sink that partitions) and a mid-batch refusal never advances past unconfirmed work; the pump wakes on the coordination `pg_notify('rasm_outbox', sink)` channel through `NpgsqlConnection.WaitAsync` on an otherwise-idle connection, with the bounded poll as the correctness floor (a missed NOTIFY costs latency, never a lost row — the cursor law owns correctness); the webhook row's `DeliveryUnconfirmed` reconciliation re-reads `net._http_response` by request-id on the NEXT drain, so a PENDING response resolves without a dedicated poller; a crash between delivery and advance re-drains the suffix and every sink's dedup column states what absorbs it (`#EGRESS_SINK`); dead-letter replay decrements nothing — the receipt's conservation fold proves `delivered + held + deadLettered == drained` on every drain.
- Receipt: a drain rides `store.egress.drain` carrying the sink, the from/through sequences, and the delivered/duplicate/held/dead-lettered counts; a dead-letter rides `store.egress.deadletter` carrying the content key and the fault; a replay rides `store.egress.replay`.
- Packages: Npgsql (`NpgsqlConnection.Notification`/`WaitAsync` — the pump wake), Marten (`IDocumentSession.Store`/`SaveChangesAsync` — the dead-letter document), Rasm (`IValidityEvidence`/`ValidityClaim`), Microsoft.Extensions.Compliance.Redaction (`IRedactorProvider.GetRedactor(DataClassificationSet)` — the classified-field gate before the boundary), LanguageExt.Core, NodaTime, Thinktecture.Runtime.Extensions, BCL inbox.
- Growth: a new delivery target is one `EgressSink` case plus its `Deliver` composition (`#EGRESS_SINK`) and one `outbox_cursor` row minted on first drain — zero pump edits; a new drain policy (batch width, wake channel, redaction set) is one `EgressPorts`/sink-row value; zero new surface — a per-sink pump, a second delivery path for replay, a fire-and-forget webhook, a presence row in the CDC drain, or a CDC poller beside the changefeed is the deleted form because the pump is one fold, replay is the same fold, the advance law owns the cursor, and the durable lanes are the only drain source.
- Boundary: the pump drains the durable outbox — `Family.Durable` lanes past the per-sink cursor — and the presence/awareness lane (`durable: false`) NEVER enters the envelope (the lossy `DrainSurface` is its only transport); the cursor-advance CAS failure is `CoordinationFault.OutboxDrain` raised by the coordination store (the fenced write is its rail), while every delivery fault is THIS band — `DeadLetter` the poisoned entry, `SinkRefused` the sink-level refusal, `CursorStall` the held cursor evidence, `DeliveryUnconfirmed` the pg_net PENDING/ERROR unconfirmed state the advance law reconciles; classified fields redact BEFORE envelope construction (`ErasingRedactor` the fail-closed fallback) so an out-of-authority payload crosses masked, never raw; caller cancellation passes through untyped; the wire-native row hands bytes to the AppHost `OutboundHop` keyed pipeline and reads its delivery-honesty policy — Persistence never owns that channel.

```csharp signature
// --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
using Rasm.Persistence.Element;                   // FaultBand — the one band registry (graph#FAULT_TABLES)
using Rasm.Persistence.Store;                     // OutboxCursor / SinkKey / OutboxAdvance (coordination#OUTBOX_CURSOR)
using Expected = Rasm.Domain.Expected;            // the federation fault-band base — NOT LanguageExt.Common.Expected

namespace Rasm.Persistence.Version;

// --- [MODELS] ---------------------------------------------------------------------------

// The typed dead-letter row: stored as a Marten document in the SAME session as the cursor state, replayed by
// content key through the one pump fold. `Attempts` gates the replay schedule; `Fault` carries the refusing ack.
public sealed record DeadLetterRow(UInt128 ContentKey, SinkKey Sink, long Sequence, string Fault, int Attempts, Instant At) {
    public Guid Id { get; init; } = Guid.CreateVersion7();
}

// The injected delegate frame the composition root fills: sink clients, the coordination advance arrow, and
// the redactor — values on a Persistence-owned shape, never an AppHost type ([A.1]).
public sealed record EgressPorts(
    Func<CoordinationOp, Option<LeaseToken>, IO<Fin<CoordinationReceipt>>> Coordinate,
    Func<ReplayWindow, IO<Seq<OpLogEntry>>> Feed,
    Func<string, string> Redact);

// The per-drain evidence on the kernel validity floor: conservation is the fold — every drained row is
// delivered, held, or dead-lettered, exactly once ([C]).
public sealed record EgressReceipt(SinkKey Sink, long From, long Through, int Drained, int Delivered, int Duplicates, int Held, int DeadLettered, Duration Elapsed, Instant At, Guid Correlation) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.Of(Through >= From),
        ValidityClaim.CountExactly(Delivered + Held + DeadLettered, Drained),
        ValidityClaim.CountAtLeast(Delivered, Duplicates));
}

// --- [ERRORS] ---------------------------------------------------------------------------
// Band 8270 (Element/graph#FAULT_TABLES registry row `Egress`): delivery faults ONLY — the failed cursor-CAS
// stays `CoordinationFault.OutboxDrain` in the fenced store's band. `DeliveryUnconfirmed` types the pg_net
// `net.request_status` PENDING/ERROR reconciliation state the advance law holds on.
[Union]
public abstract partial record EgressFault : Expected, IValidationError<EgressFault> {
    private EgressFault() : base() { }

    public sealed record DeadLetter(UInt128 ContentKey, SinkKey Sink, string Detail) : EgressFault;
    public sealed record SinkRefused(SinkKey Sink, string Detail) : EgressFault;
    public sealed record CursorStall(SinkKey Sink, long Held) : EgressFault;
    public sealed record DeliveryUnconfirmed(SinkKey Sink, long RequestId) : EgressFault;

    public override int Code => FaultBand.Egress + Switch(
        deadLetter:          static _ => 1,
        sinkRefused:         static _ => 2,
        cursorStall:         static _ => 3,
        deliveryUnconfirmed: static _ => 4);

    public override string Message => Switch(
        deadLetter:          static c => $"<dead-letter:{c.Sink.Value}:{c.ContentKey:x32}>:{c.Detail}",
        sinkRefused:         static c => $"<sink-refused:{c.Sink.Value}>:{c.Detail}",
        cursorStall:         static c => $"<cursor-stall:{c.Sink.Value}@{c.Held}>",
        deliveryUnconfirmed: static c => $"<delivery-unconfirmed:{c.Sink.Value}#{c.RequestId}>");

    public override string Category => Switch(
        deadLetter:          static _ => "DeadLetter",
        sinkRefused:         static _ => "Sink",
        cursorStall:         static _ => "Cursor",
        deliveryUnconfirmed: static _ => "Unconfirmed");

    public static EgressFault Create(string message) => new SinkRefused(SinkKey.Create("<none>"), message);
}

// --- [OPERATIONS] -----------------------------------------------------------------------

public static class EgressPump {
    // The one drain fold: windowed rows -> envelope -> sink delivery -> DeliveryAck -> contiguous-prefix
    // cursor advance. The first Indeterminate HOLDS the cursor (re-drain; sink dedup absorbs the replay); a
    // Refused dead-letters in the same session and the drain continues. Replay is this fold re-parameterized.
    public static IO<Fin<EgressReceipt>> Drain(EgressSink sink, OutboxCursor cursor, EgressPorts ports, ProjectionContext frame) =>
        from mark in IO.lift(frame.Mark)
        from rows in ports.Feed(ReplayWindow.DurableOps(cursor.Sequence, sink.Batch))
        from folded in rows.Fold(
            IO.pure((Through: cursor.Sequence, Delivered: 0, Duplicates: 0, Held: 0, Dead: 0, Open: true)),
            (acc, row) => acc.Bind(state => !state.Open
                ? IO.pure(state with { Held = state.Held + 1 })
                : sink.Deliver(Egress.Envelope(row, ports.Redact), row).Map(ack => ack.Switch(
                    persisted:     p => state with { Through = row.Sequence, Delivered = state.Delivered + 1, Duplicates = state.Duplicates + (p.Duplicate ? 1 : 0) },
                    indeterminate: _ => state with { Held = state.Held + 1, Open = false },
                    refused:       _ => state with { Dead = state.Dead + 1 }))))
        from _ in folded.Through > cursor.Sequence
            ? ports.Coordinate(new CoordinationOp.OutboxAdvance(sink.Key, folded.Through), sink.Held)
            : IO.pure(Fin<CoordinationReceipt>.Succ(default!))
        select Fin<EgressReceipt>.Succ(new EgressReceipt(sink.Key, cursor.Sequence, folded.Through, rows.Count,
            folded.Delivered, folded.Duplicates, folded.Held, folded.Dead, frame.Elapsed(mark), frame.Now(), frame.Correlation));
}
```

| [INDEX] | [POLICY]          | [VALUE]                                        | [BINDING]                                                        |
| :-----: | :---------------- | :--------------------------------------------- | :---------------------------------------------------------------- |
|  [01]   | drain source      | `ReplayWindow.DurableOps` past the sink cursor | one windowed read (ledger); presence never enters                |
|  [02]   | advance law       | contiguous `Persisted` prefix only             | Indeterminate holds; Refused dead-letters and continues          |
|  [03]   | replay            | the same fold over `DeadLetterRow` keys        | never a second delivery path                                     |
|  [04]   | wake              | `WaitAsync` on `rasm_outbox` + bounded poll    | NOTIFY is latency; the poll floor owns correctness               |
|  [05]   | redaction         | `Redact` before envelope construction          | fail-closed `ErasingRedactor`; classified fields never cross raw |
|  [06]   | receipt floor     | conservation `ValidityClaim.All` fold          | delivered + held + dead == drained, exactly once ([C])           |

## [03]-[EGRESS_SINK]

- Owner: `CdcEnvelope` the one CloudEvents projection of an `OpLogEntry` — `Egress.Envelope` populates required `Id` (the content key, lower-hex), `Source` (`rasm:persistence/oplog`), `Type` (`rasm.oplog.{family}.{kind}`), `Time` (the entry's `Physical` instant), `Data` (the redacted payload bytes, `application/octet-stream`), `Partitioning.SetPartitionKey(EntityKey)`, `Sequence.SetSequence(entry.Sequence)`, and the `traceparent`/`redacted` extension attributes declared ONCE via `CloudEventAttribute.CreateExtension` — one envelope for every sink, one shared `JsonEventFormatter` whose `JsonSerializerOptions` compose `ConfigureForNodaTime(DateTimeZoneProviders.Tzdb)` + `ThinktectureJsonConverterFactory` (disjoint type-spaces, co-registered once at construction); `EgressSink` the closed `[Union]` delivery-row family whose every case carries its dedup stance, batch width, and held token as row DATA; `DeliveryAck` the one `[Union]` every sink outcome folds to at the sink's own boundary — a raw `PubAckResponse`/`DeliveryResult`/`MessageId` never crosses into the pump.
- Cases: `Webhook(Uri Url, HashMap<string, string> Headers)` — enqueue `net.http_post(url, body, headers + idempotency-key = ContentKey-hex)` returning the `bigint` request-id, fold `net.http_response_result` on the NEXT drain: `SUCCESS` → `Persisted`, `PENDING` → `Indeterminate` (`DeliveryUnconfirmed` evidence), `ERROR`/timeout → `Refused`; a pg_net UNLOGGED-table crash loses response rows, so the held cursor re-posts under the SAME idempotency-key header — receiver-side dedup, the row's honest stance. `Nats(string Subject)` — `INatsJSContext.PublishAsync` with `NatsHeaders["Nats-Msg-Id"] = ContentKey-hex`; the awaited `PubAckResponse` is the Settle-ack (`Persisted`), `Duplicate: true` the dedup-window replay absorbed (`Persisted(Duplicate: true)`), a thrown/`-ERR` outcome `Refused` via the `TryPublishAsync` ROP form. `Kafka(string Topic, bool Sealed)` — `cloudEvent.ToKafkaMessage(ContentMode.Binary, formatter)` (attributes ride `ce_*` headers, `partitionkey` projects onto `Message.Key`), awaited `ProduceAsync`: `Status == Persisted` → `Persisted`, `PossiblyPersisted` → `Indeterminate`, error → `Refused`; `Sealed: true` composes the producer transaction (`InitTransactions` → `BeginTransaction` → produce batch → `SendOffsetsToTransaction` → `CommitTransaction`) so the batch and the cursor offset commit atomically — real exactly-once as a policy value, not a new sink. `RabbitMq(string Exchange, string RoutingKey)` — `CreateChannelOptions(publisherConfirmationsEnabled: true, publisherConfirmationTrackingEnabled: true)` then awaited `BasicPublishAsync` (the await IS the confirm): completion → `Persisted`, nack/return → `Refused`. `Pulsar(string Topic)` — `ISend.Send(metadata, payload)` → `MessageId` → `Persisted`; metadata carries the content key as the dedup property. `WireNative(string HopKey)` — Persistence writes `MessageExtensions.WriteLengthPrefixedTo` bytes onto the AppHost `OutboundHop` keyed pipeline and folds the hop's delivery-honesty verdict; the gRPC channel is AppHost-owned. `RedisStream(string Stream, string Group)` — `StreamAdd(stream, fields, StreamIdempotentId(ContentKey-hex), trimMode: StreamTrimMode.Acknowledged)` (producer-side at-most-once XADD — the idempotent-id overload, stronger than receiver-side dedup and composed because the capability exists); consumers `StreamReadGroup`/`StreamAcknowledge`, the group ack the delivery confirmation that advances the cursor; the zero-broker-install stream row.
- Entry: `public IO<DeliveryAck> Deliver(CloudEvent envelope, OpLogEntry row)` on the sink row — each case's composition converts its provider outcome to `DeliveryAck` at the sink boundary (ONE conversion site per sink), and `public SinkBinding Binding` names the transport lane (`Subject`/`Topic`/`Stream`/`Url`) as data for telemetry and the cursor row key.
- Auto: dedup honesty is a COLUMN, not prose — every case states what absorbs a replay: NATS the broker dedup window on `Nats-Msg-Id`, Kafka idempotent-producer plus consumer id-dedup (or the `Sealed` transaction), webhook/pulsar/wire-native receiver-side id-dedup on the CloudEvents `id`, redis the producer-side `StreamIdempotentId`; the envelope is `ContentMode.Binary` everywhere a header-bearing transport exists so a broker filters on `ce_type`/`ce_source`/`partitionkey` without parsing the body; serdes-governed Kafka bodies (Confluent Schema Registry Avro/Json/Protobuf) own the `Data` bytes and their schema-id framing beside the `ce_*` envelope headers with zero key collision — envelope codec and body codec never share a `JsonSerializerOptions`.
- Receipt: per-sink delivery evidence rides the drain receipt (`#EGRESS_PUMP`); the sink names its lane through `Binding`, never a free string.
- Packages: CloudNative.CloudEvents (+`.SystemTextJson` `JsonEventFormatter`, +`.Kafka` `ToKafkaMessage`; `Partitioning`/`Sequence` extension attributes), NATS.Net (`INatsJSContext.PublishAsync`/`TryPublishAsync`, `NatsHeaders`, `PubAckResponse.Duplicate`), Confluent.Kafka (`ProduceAsync`, `DeliveryResult.Status`, `InitTransactions`/`SendOffsetsToTransaction`/`CommitTransaction`) + Confluent.SchemaRegistry serdes rows, RabbitMQ.Client (`CreateChannelOptions` confirms, `BasicPublishAsync`), DotPulsar (`ISend.Send` → `MessageId`), StackExchange.Redis (`StreamAdd`/`StreamIdempotentId`/`StreamReadGroup`/`StreamAcknowledge`/`StreamTrimMode.Acknowledged`), pg_net (`net.http_post`/`net.request_status`/`net.http_response_result` over raw Npgsql), Google.Protobuf (`WriteLengthPrefixedTo` — the wire-native payload), NodaTime.Serialization.SystemTextJson (`ConfigureForNodaTime` on the one formatter options), Thinktecture.Runtime.Extensions.Json (`ThinktectureJsonConverterFactory` co-registered), BCL inbox.
- Growth: a new delivery target is ONE `EgressSink` case carrying its `Deliver` composition and dedup column — the pump, the envelope, the cursor, and the receipt are untouched; a new envelope attribute is one `CloudEventAttribute.CreateExtension` declaration; zero new surface — a per-sink envelope shape, a hand-built `ce_` header, a raw provider ack crossing into the pump, a second formatter, or a fire-and-forget publish on a durable row is the deleted form.
- Boundary: the envelope is the single cross-consumer, cross-language vocabulary — the AppHost outbox relay and the durable-orchestration dispatch drain the SAME CloudEvents projection as their hop payload, so a per-consumer re-pack is the drift defect; `id` is the content key so replay dedup is content identity, never a broker sequence; the webhook row NEVER fire-and-forgets — `net.http_post` enqueues and the response reconciliation is the only advance authority; the wire-native row reads the AppHost delivery-honesty policy and never re-implements hop retry (the database is excluded from the AppHost hop law; sink delivery is not); the redis row's `StreamTrimMode.Acknowledged` trim keeps the stream bounded by consumption, never a time guess.

```csharp signature
// --- [TYPES] ----------------------------------------------------------------------------

// ONE ack family every sink outcome folds to at the sink's own boundary — a raw PubAckResponse /
// DeliveryResult / MessageId / request_status never crosses into the pump. Only Persisted advances.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record DeliveryAck {
    private DeliveryAck() { }
    public sealed record Persisted(bool Duplicate) : DeliveryAck;
    public sealed record Indeterminate(string Detail) : DeliveryAck;
    public sealed record Refused(string Detail) : DeliveryAck;

    public static DeliveryAck FromResult(PersistenceStatus status, string detail) => status switch {
        PersistenceStatus.Persisted         => new Persisted(Duplicate: false),
        PersistenceStatus.PossiblyPersisted => new Indeterminate(detail),
        _                                   => new Refused(detail),
    };
}

// The closed delivery-row family: every case carries its transport binding, batch width, held fencing token,
// and dedup stance as row DATA — a new sink is one case, zero pump edits.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record EgressSink {
    private EgressSink() { }

    public sealed record Webhook(SinkKey Key, Uri Url, HashMap<string, string> Headers, int Batch, Option<LeaseToken> Held) : EgressSink;
    public sealed record Nats(SinkKey Key, string Subject, int Batch, Option<LeaseToken> Held) : EgressSink;
    public sealed record Kafka(SinkKey Key, string Topic, bool Sealed, int Batch, Option<LeaseToken> Held) : EgressSink;
    public sealed record RabbitMq(SinkKey Key, string Exchange, string RoutingKey, int Batch, Option<LeaseToken> Held) : EgressSink;
    public sealed record Pulsar(SinkKey Key, string Topic, int Batch, Option<LeaseToken> Held) : EgressSink;
    public sealed record WireNative(SinkKey Key, string HopKey, int Batch, Option<LeaseToken> Held) : EgressSink;
    public sealed record RedisStream(SinkKey Key, string Stream, string Group, int Batch, Option<LeaseToken> Held) : EgressSink;
}

// --- [OPERATIONS] -----------------------------------------------------------------------

public static class Egress {
    // Declared once, set per event — typed extension attributes, never hand-built ce_ header strings.
    static readonly CloudEventAttribute TraceParent = CloudEventAttribute.CreateExtension("traceparent", CloudEventAttributeType.String);
    static readonly CloudEventAttribute Redacted = CloudEventAttribute.CreateExtension("redacted", CloudEventAttributeType.Boolean);

    // The ONE envelope rail: id = content key, source = the oplog URI, type = family.kind, partitionkey =
    // EntityKey (per-entity order inside every partitioning sink), sequence = the changefeed sequence. The
    // payload redacts BEFORE the envelope leaves the trust boundary; the trace slot continues as traceparent.
    public static CloudEvent Envelope(OpLogEntry row, Func<string, string> redact) {
        var ce = new CloudEvent {
            Id = row.ContentKey.ToString("x32"),
            Source = new Uri("rasm:persistence/oplog"),
            Type = $"rasm.oplog.{row.Family.Key}.{row.Kind.Key}",
            Time = row.Physical.ToDateTimeOffset(),
            DataContentType = "application/octet-stream",
            Data = row.Payload,
        };
        Partitioning.SetPartitionKey(ce, redact(row.EntityKey));
        Sequence.SetSequence(ce, row.Sequence);
        row.Trace.Continue().IfSome(ctx => ce[TraceParent] = $"00-{ctx.TraceId}-{ctx.SpanId}-01");
        return ce;
    }
}
```

| [INDEX] | [SINK]        | [DEDUP / ADVANCE]                                            | [REPLAY ABSORBED BY]                          |
| :-----: | :------------ | :----------------------------------------------------------- | :--------------------------------------------- |
|  [01]   | webhook       | `net.request_status = SUCCESS` reconciliation only            | idempotency-key header (receiver-side)          |
|  [02]   | nats          | JetStream `PubAckResponse` Settle-ack                         | broker dedup window on `Nats-Msg-Id`            |
|  [03]   | kafka         | `DeliveryResult.Status == Persisted`; `Sealed` = producer txn | idempotent producer + consumer id-dedup / txn   |
|  [04]   | rabbitmq      | awaited publisher confirm                                     | receiver-side id-dedup on CloudEvents `id`      |
|  [05]   | pulsar        | `ISend.Send` → `MessageId`                                    | receiver-side id-dedup on CloudEvents `id`      |
|  [06]   | wire-native   | AppHost `OutboundHop` delivery-honesty verdict                | hop policy (AppHost-owned)                      |
|  [07]   | redis-stream  | consumer-group `StreamAcknowledge`                            | producer-side `StreamIdempotentId` (at-most-once XADD) |

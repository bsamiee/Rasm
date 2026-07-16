# [PERSISTENCE_VERSION_EGRESS]

`EgressPump` drains durable `OpLogEntry` rows past each sink cursor, projects one CloudEvents envelope, folds every provider result into `DeliveryAck`, and advances only a confirmed contiguous prefix. `EgressSink` closes the delivery targets and exposes their provider settings to the bound adapter. Content identity supplies the replay key; each sink row declares the mechanism that absorbs replay. Presence and awareness never enter this durable rail.

## [01]-[INDEX]

- [01]-[EGRESS_PUMP]: the one drain fold over the per-sink cursor, the advance law, the typed dead-letter and replay rows, the `EgressReceipt` validity fold, and the 8270 fault band.
- [02]-[EGRESS_SINK]: the `CdcEnvelope` CloudEvents projection, the closed `EgressSink` delivery-row family, the `DeliveryAck` fold, and the per-sink dedup honesty table.

## [02]-[EGRESS_PUMP]

- Owner: `EgressPump` the static surface owning the one drain bracket — notification wait, cursor read, windowed row drain, envelope projection, sink delivery, ack fold, cursor advance, dead-letter capture; `DeadLetterRow` the typed dead-letter document (content key, sink, sequence, fault, attempt count) stored in the SAME Marten session so a dead-letter and its cursor state commit atomically; `EgressReceipt` the per-drain evidence implementing the kernel `IValidityEvidence`; `EgressFault` the 8270 band; `EgressPorts` the injected delegate frame (`Wait` binds `NpgsqlConnection.WaitAsync` plus its bounded poll, then feed, coordination, redaction, and session-bound dead-letter arrows) filled at the composition root.
- Entry: `public static IO<Fin<EgressReceipt>> Drain(EgressSink sink, OutboxCursor cursor, EgressPorts ports, ProjectionContext frame)` is the one pump — it drains `ReplayWindow.DurableOps(cursor.Sequence, sink.Binding.Batch)` rows from the changefeed (the `Version/ledger` windowed-read case parameterized for this drain — never a third read surface), projects each row through `Egress.Envelope`, delivers through the sink row's composition, folds every outcome to `DeliveryAck`, and advances the cursor through `Store/coordination` `OutboxAdvance(sink.Binding.Key, through)` ONLY past the contiguous `Persisted` prefix — the first `Indeterminate` holds the cursor at its predecessor (a held cursor re-drains, the sink's dedup absorbs the replay), a `Refused` writes the `DeadLetterRow` and the drain continues past it; `public static IO<Fin<EgressReceipt>> Replay(EgressSink sink, Seq<DeadLetterRow> letters, EgressPorts ports, ProjectionContext frame)` re-delivers dead-lettered entries by content key through the SAME envelope/deliver/ack fold — replay is the pump re-parameterized, never a second delivery path.
- Auto: the drain is one fold per batch — `rows.Map(Egress.Envelope).Map(sink.Deliver)` folded left with the contiguous-prefix advance accumulator, so ordering per partition key is preserved (the envelope's `partitionkey` = `EntityKey` keeps per-entity order inside every sink that partitions) and a mid-batch refusal never advances past unconfirmed work; the pump wakes on the coordination `pg_notify('rasm_outbox', sink)` channel through `NpgsqlConnection.WaitAsync` on an otherwise-idle connection, with the bounded poll as the correctness floor (a missed NOTIFY costs latency, never a lost row — the cursor law owns correctness); the webhook row's `DeliveryUnconfirmed` reconciliation re-reads `net._http_response` by request-id on the NEXT drain, so a PENDING response resolves without a dedicated poller; a crash between delivery and advance re-drains the suffix and every sink's dedup column states what absorbs it (`#EGRESS_SINK`); dead-letter replay decrements nothing — the receipt's conservation fold proves `delivered + held + deadLettered == drained` on every drain.
- Receipt: a drain rides `store.egress.drain` carrying the sink, the from/through sequences, and the delivered/duplicate/held/dead-lettered counts; a dead-letter rides `store.egress.deadletter` carrying the content key and the fault; a replay rides `store.egress.replay`.
- Packages: Npgsql (`NpgsqlConnection.Notification`/`WaitAsync` — the pump wake), Marten (`IDocumentSession.Store`/`SaveChangesAsync` — the dead-letter document), Rasm (`IValidityEvidence`/`ValidityClaim`), Microsoft.Extensions.Compliance.Redaction (`IRedactorProvider.GetRedactor(DataClassificationSet)` — the classified-field gate before the boundary), LanguageExt.Core, NodaTime, Thinktecture.Runtime.Extensions, BCL inbox.
- Growth: a new delivery target is one `EgressSink` case plus its `Deliver` composition (`#EGRESS_SINK`) and one `outbox_cursor` row minted on first drain — zero pump edits; a new drain policy (batch width, wake channel, redaction set) is one `EgressPorts`/sink-row value; zero new surface — a per-sink pump, a second delivery path for replay, a fire-and-forget webhook, a presence row in the CDC drain, or a CDC poller beside the changefeed is the deleted form because the pump is one fold, replay is the same fold, the advance law owns the cursor, and the durable lanes are the only drain source.
- Boundary: the pump drains the durable outbox — `Family.Durable` lanes past the per-sink cursor — and the presence/awareness lane (`durable: false`) NEVER enters the envelope (the lossy `DrainSurface` is its only transport); the cursor-advance CAS failure is `CoordinationFault.OutboxDrain` raised by the coordination store (the fenced write is its rail), while every delivery fault is THIS band — `DeadLetter` the poisoned entry, `SinkRefused` the sink-level refusal, `CursorStall` the held cursor evidence, `DeliveryUnconfirmed` the pg_net PENDING/ERROR unconfirmed state the advance law reconciles; classified fields redact BEFORE envelope construction (`ErasingRedactor` the fail-closed fallback) so an out-of-authority payload crosses masked, never raw; caller cancellation passes through untyped; the wire-native row hands bytes to the AppHost `OutboundHop` keyed pipeline and reads its delivery-honesty policy — Persistence never owns that channel.

```csharp signature
// --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
using Rasm.Persistence.Element;                   // FaultBand — the one band registry (graph#FAULT_TABLES)
using Rasm.Persistence.Store;                     // OutboxCursor / SinkKey / OutboxAdvance (coordination#OUTBOX_CURSOR)

namespace Rasm.Persistence.Version;

// --- [MODELS] ---------------------------------------------------------------------------

// The typed dead-letter row: stored as a Marten document in the SAME session as the cursor state, replayed by
// content key through the one pump fold. `Attempts` gates the replay schedule; `Fault` carries the refusing ack.
public sealed record DeadLetterRow(UInt128 ContentKey, SinkKey Sink, long Sequence, string Fault, int Attempts, Instant At) {
    public Guid Id { get; init; } = Guid.CreateVersion7();
}

// The injected delegate frame the composition root fills: sink clients, the coordination advance arrow, the
// payload redactor, and the dead-letter store/retire arrows — values on a Persistence-owned shape, never an
// AppHost type ([A.1]). `DeadLetter`/`Retire` close over the SAME Marten `IDocumentSession` as the cursor
// state (`Store` then `SaveChangesAsync` with the drain), so a letter and its drain commit atomically; `Redact`
// masks the row's classified payload fields (built from `IRedactorProvider.GetRedactor(DataClassificationSet)`,
// `ErasingRedactor` the fail-closed fallback) and reports whether masking fired.
public sealed record EgressPorts(
    Func<IO<Unit>> Wait,
    Func<CoordinationOp, Option<LeaseToken>, IO<Fin<CoordinationReceipt>>> Coordinate,
    Func<ReplayWindow, IO<Seq<OpLogEntry>>> Feed,
    Func<OpLogEntry, (ReadOnlyMemory<byte> Data, bool Redacted)> Redact,
    Func<DeadLetterRow, IO<Unit>> DeadLetter,
    Func<DeadLetterRow, IO<Unit>> Retire);

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
public abstract partial record EgressFault : Rasm.Domain.Expected, IValidationError<EgressFault> {
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
    // Refused PERSISTS its DeadLetterRow through the session-bound port BEFORE counting, and the cursor
    // advances past it — the durable letter, not the cursor, owns the poisoned entry from that point.
    public static IO<Fin<EgressReceipt>> Drain(EgressSink sink, OutboxCursor cursor, EgressPorts ports, ProjectionContext frame) =>
        from mark in IO.lift(frame.Mark)
        from _ in ports.Wait()
        from rows in ports.Feed(ReplayWindow.DurableOps(cursor.Sequence, sink.Binding.Batch))
        from folded in rows.FoldM(
            (Through: cursor.Sequence, Delivered: 0, Duplicates: 0, Held: 0, Dead: 0, Open: true),
            (state, row) => !state.Open
                ? IO.pure(state with { Held = state.Held + 1 })
                : sink.Deliver(Egress.Envelope(row, ports.Redact), row).Bind(ack => ack.Switch(
                    persisted:     p  => IO.pure(state with { Through = row.Sequence, Delivered = state.Delivered + 1, Duplicates = state.Duplicates + (p.Duplicate ? 1 : 0) }),
                    indeterminate: _  => IO.pure(state with { Held = state.Held + 1, Open = false }),
                    refused:       rf => ports.DeadLetter(new DeadLetterRow(row.ContentKey, sink.Binding.Key, row.Sequence, rf.Detail, 1, frame.Now()))
                                             .Map(_ => state with { Through = row.Sequence, Dead = state.Dead + 1 })))).As()
        from advance in folded.Through > cursor.Sequence
            ? ports.Coordinate(new CoordinationOp.OutboxAdvance(sink.Binding.Key, folded.Through), sink.Binding.Held)
            : IO.pure(Fin<CoordinationReceipt>.Succ(default!))
        let receipt = new EgressReceipt(sink.Binding.Key, cursor.Sequence, folded.Through, rows.Count, folded.Delivered, folded.Duplicates, folded.Held, folded.Dead, frame.Elapsed(mark), frame.Now(), frame.Correlation)
        select advance.Match(Succ: _ => Fin<EgressReceipt>.Succ(receipt), Fail: error => Fin<EgressReceipt>.Fail(error));

    // Replay IS the drain fold re-parameterized over the letter set — never a second delivery path. Each
    // letter re-reads its row through the ONE windowed feed (the singleton window at Sequence-1), re-delivers
    // through the same envelope/leg, and a Persisted retires the letter; a still-refusing row re-letters with
    // Attempts+1 (the replay schedule's gate); a vanished row (retention-swept) retires as Held — the
    // conservation fold closes over letters exactly as the drain closes over rows.
    public static IO<Fin<EgressReceipt>> Replay(EgressSink sink, Seq<DeadLetterRow> letters, EgressPorts ports, ProjectionContext frame) =>
        from mark in IO.lift(frame.Mark)
        from folded in letters.FoldM(
            (Delivered: 0, Duplicates: 0, Held: 0, Dead: 0),
            (state, letter) =>
                from rows in ports.Feed(ReplayWindow.DurableOps(letter.Sequence - 1, 1))
                let found = rows.Filter(r => r.ContentKey == letter.ContentKey).HeadOrNone()
                from next in found.Match(
                    Some: row => from ack in sink.Deliver(Egress.Envelope(row, ports.Redact), row)
                                 from settled in ack.Switch(
                        persisted:     p  => ports.Retire(letter).Map(_ => state with { Delivered = state.Delivered + 1, Duplicates = state.Duplicates + (p.Duplicate ? 1 : 0) }),
                        indeterminate: _  => IO.pure(state with { Held = state.Held + 1 }),
                        refused:       rf => ports.DeadLetter(letter with { Fault = rf.Detail, Attempts = letter.Attempts + 1, At = frame.Now() }).Map(_ => state with { Dead = state.Dead + 1 }))
                                 select settled,
                    None: () => ports.Retire(letter).Map(_ => state with { Held = state.Held + 1 }))
                select next).As()
        select Fin<EgressReceipt>.Succ(new EgressReceipt(sink.Binding.Key, 0, 0, letters.Count,
            folded.Delivered, folded.Duplicates, folded.Held, folded.Dead, frame.Elapsed(mark), frame.Now(), frame.Correlation));
}
```

| [INDEX] | [POLICY]      | [VALUE]                                        | [BINDING]                                                        |
| :-----: | :------------ | :--------------------------------------------- | :--------------------------------------------------------------- |
|  [01]   | drain source  | `ReplayWindow.DurableOps` past the sink cursor | one windowed read (ledger); presence never enters                |
|  [02]   | advance law   | contiguous `Persisted` prefix only             | Indeterminate holds; Refused dead-letters and continues          |
|  [03]   | replay        | the same fold over `DeadLetterRow` keys        | never a second delivery path                                     |
|  [04]   | wake          | `WaitAsync` on `rasm_outbox` + bounded poll    | NOTIFY is latency; the poll floor owns correctness               |
|  [05]   | redaction     | `Redact` before envelope construction          | fail-closed `ErasingRedactor`; classified fields never cross raw |
|  [06]   | receipt floor | conservation `ValidityClaim.All` fold          | delivered + held + dead == drained, exactly once ([C])           |

## [03]-[EGRESS_SINK]

- Owner: `CdcEnvelope` the one CloudEvents projection of an `OpLogEntry` — `Egress.Envelope` populates required `Id` (the content key, lower-hex), `Source` (`rasm:persistence/oplog`), `Type` (`rasm.oplog.{family}.{kind}`), `Time` (the entry's `Physical` instant), `Data` (the redacted payload bytes, `application/octet-stream`), `Partitioning.SetPartitionKey(EntityKey)`, `Sequence.SetSequence(entry.Sequence.ToString(InvariantCulture))`, and the `traceparent`/`redacted` extension attributes declared ONCE via `CloudEventAttribute.CreateExtension` — one envelope for every sink, one shared `JsonEventFormatter` whose `JsonSerializerOptions` compose `ConfigureForNodaTime(DateTimeZoneProviders.Tzdb)` + `ThinktectureJsonConverterFactory` (disjoint type-spaces, co-registered once at construction); `EgressSink` the closed `[Union]` delivery-row family whose every case carries its dedup stance, batch width, and held token as row DATA; `DeliveryAck` the one `[Union]` every sink outcome folds to at the sink's own boundary — a raw `PubAckResponse`/`DeliveryResult`/`MessageId` never crosses into the pump.
- Cases: `Webhook(Uri Url, HashMap<string, string> Headers)` — enqueue `net.http_post(url, body, headers + idempotency-key = ContentKey-hex)` returning the `bigint` request-id, fold `net.http_response_result` on the NEXT drain: `SUCCESS` → `Persisted`, `PENDING` → `Indeterminate` (`DeliveryUnconfirmed` evidence), `ERROR`/timeout → `Refused`; a pg_net UNLOGGED-table crash loses response rows, so the held cursor re-posts under the SAME idempotency-key header — receiver-side dedup, the row's honest stance. `Nats(string Subject)` — `INatsJSContext.PublishAsync` with `NatsHeaders["Nats-Msg-Id"] = ContentKey-hex`; the awaited `PubAckResponse` is the Settle-ack (`Persisted`), `Duplicate: true` the dedup-window replay absorbed (`Persisted(Duplicate: true)`), a server `-ERR`/timeout `Indeterminate` (retriable — the held cursor re-drives into the dedup window, `api-nats` JETSTREAM_DELIVERY_ACK) and only a fatal protocol fault `Refused`, via the `TryPublishAsync` ROP form. `Kafka(string Topic, KafkaPublishMode Mode)` — `cloudEvent.ToKafkaMessage(ContentMode.Binary, formatter)` (attributes ride `ce_*` headers, `partitionkey` projects onto `Message.Key`); `Awaited` folds the awaited `ProduceAsync` broker ack (`Status == Persisted` → `Persisted`, `NotPersisted`/`PossiblyPersisted` → `Indeterminate`, caught fatal `ProduceException` → `Refused`), while `ReadCommitted` brackets the publish with `InitTransactions` → `BeginTransaction` → `CommitTransaction` so `isolation.level=read_committed` consumers never observe an aborted record. Kafka transactions cannot commit the PostgreSQL outbox cursor; every mode remains at-least-once across that boundary and content-key consumer dedup absorbs a crash after broker persistence but before cursor advance. `RabbitMq(string Exchange, string RoutingKey)` — `CreateChannelOptions(publisherConfirmationsEnabled: true, publisherConfirmationTrackingEnabled: true)` then awaited `BasicPublishAsync` (the await IS the confirm): completion → `Persisted`, nack/return → `Refused`. `Pulsar(string Topic)` — `ISend.Send(metadata, payload)` → `MessageId` → `Persisted`; metadata carries the content key as the dedup property. `WireNative(string HopKey)` — Persistence writes `MessageExtensions.WriteLengthPrefixedTo` bytes onto the AppHost `OutboundHop` keyed pipeline and folds the hop's delivery-honesty verdict; the gRPC channel is AppHost-owned. `RedisStream(string Stream, string Group)` — await `StreamAdd(stream, fields, StreamIdempotentId(ContentKey-hex), trimMode: StreamTrimMode.Acknowledged)`; a returned stream id is `Persisted`, and a transport ambiguity is `Indeterminate`. Downstream consumers own `StreamReadGroup`/`StreamAcknowledge`; their independent group cursor never governs the PostgreSQL outbox cursor. `ClickHouse(string Table)` — `ClickHouseClient.InsertBinaryAsync` (the pooled RowBinary ingest rail) with the `insert_deduplication_token` server setting = ContentKey-hex riding the `InsertOptions`/`QueryOptions` custom-settings row (the producer-supplied dedup id ClickHouse deduplicates replays on — exactly the content-key dedup stance the envelope law demands; `BeginDbTransaction` throws `NotSupportedException`, so the token IS the sink's whole dedup story), the awaited insert completion → `Persisted`, a transient server/connection fault → `Indeterminate` (the held cursor re-drives under the SAME token, absorbed), a schema/table rejection → `Refused`; the billion-row fleet-analytics lane whose scale-out read residence the `Query/columnar` `clickhouse-scaleout` axis row consumes.
- Entry: `public IO<DeliveryAck> Deliver(CloudEvent envelope, OpLogEntry row)` resolves to the case's bound `SinkBinding.Leg` (each leg is ONE conversion site folding its provider outcome to `DeliveryAck`, filled at the composition root from the provider client), and `public SinkBinding Binding` derives the shared `(Key, Batch, Held)` row data the pump and the cursor read.
- Auto: dedup honesty is a COLUMN, not prose — every case states what absorbs a replay: NATS the broker dedup window on `Nats-Msg-Id`, Kafka the idempotent producer plus content-key consumer dedup, webhook/pulsar/wire-native receiver-side id-dedup on the CloudEvents `id`, redis the producer-side `StreamIdempotentId`, clickhouse the producer-side `insert_deduplication_token`; the envelope is `ContentMode.Binary` everywhere a header-bearing transport exists so a broker filters on `ce_type`/`ce_source`/`partitionkey` without parsing the body; serdes-governed Kafka bodies (Confluent Schema Registry Avro/Json/Protobuf) own the `Data` bytes and their schema-id framing beside the `ce_*` envelope headers with zero key collision — envelope codec and body codec never share a `JsonSerializerOptions`.
- Receipt: per-sink delivery evidence rides the drain receipt (`#EGRESS_PUMP`); the sink names its lane through `Binding`, never a free string.
- Packages: CloudNative.CloudEvents (+`.SystemTextJson` `JsonEventFormatter`, +`.Kafka` `ToKafkaMessage`; `Partitioning`/`Sequence` extension attributes), NATS.Net (`INatsJSContext.PublishAsync`/`TryPublishAsync`, `NatsHeaders`, `PubAckResponse.Duplicate`), Confluent.Kafka (`ProduceAsync`, `DeliveryResult.Status`, `InitTransactions`/`BeginTransaction`/`CommitTransaction`) + Confluent.SchemaRegistry serdes rows, RabbitMQ.Client (`CreateChannelOptions` confirms, `BasicPublishAsync`), DotPulsar (`ISend.Send` → `MessageId`), StackExchange.Redis (`StreamAdd`/`StreamIdempotentId`/`StreamTrimMode.Acknowledged`), ClickHouse.Driver (`ClickHouseClient.InsertBinaryAsync` + `InsertOptions`/`QueryOptions` custom settings — the warehouse leg under the `insert_deduplication_token` producer-supplied dedup setting), pg_net (`net.http_post`/`net.request_status`/`net.http_response_result` over raw Npgsql), Google.Protobuf (`WriteLengthPrefixedTo` — the wire-native payload), NodaTime.Serialization.SystemTextJson (`ConfigureForNodaTime` on the one formatter options), Thinktecture.Runtime.Extensions.Json (`ThinktectureJsonConverterFactory` co-registered), BCL inbox.
- Growth: a new delivery target is ONE `EgressSink` case carrying its `Deliver` composition and dedup column — the pump, the envelope, the cursor, and the receipt are untouched; a new envelope attribute is one `CloudEventAttribute.CreateExtension` declaration; zero new surface — a per-sink envelope shape, a hand-built `ce_` header, a raw provider ack crossing into the pump, a second formatter, or a fire-and-forget publish on a durable row is the deleted form.
- Boundary: the envelope is the single cross-consumer, cross-language vocabulary — the AppHost outbox relay and the durable-orchestration dispatch drain the SAME CloudEvents projection as their hop payload, so a per-consumer re-pack is the drift defect; `id` is the content key so replay dedup is content identity, never a broker sequence; the webhook row NEVER fire-and-forgets — `net.http_post` enqueues and the response reconciliation is the only advance authority; the wire-native row reads the AppHost delivery-honesty policy and never re-implements hop retry (the database is excluded from the AppHost hop law; sink delivery is not); the redis row's `StreamTrimMode.Acknowledged` trim keeps the stream bounded by consumption, never a time guess.

```csharp signature
// --- [TYPES] ----------------------------------------------------------------------------

// Kafka publication mode controls broker visibility only; the PostgreSQL cursor remains outside the transaction.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class KafkaPublishMode {
    public static readonly KafkaPublishMode Awaited = new("awaited");
    public static readonly KafkaPublishMode ReadCommitted = new("read-committed");
}

// ONE ack family every sink outcome folds to at the sink's own boundary — a raw PubAckResponse /
// DeliveryResult / MessageId / request_status never crosses into the pump. Only Persisted advances.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record DeliveryAck {
    private DeliveryAck() { }
    public sealed record Persisted(bool Duplicate) : DeliveryAck;
    public sealed record Indeterminate(string Detail) : DeliveryAck;
    public sealed record Refused(string Detail) : DeliveryAck;

    // NotPersisted AND PossiblyPersisted are RETRIABLE (the api-kafka broker-ack fold): a definitively-not-
    // persisted row re-drives under the held cursor with zero duplication risk; Refused is reserved for the
    // caught ProduceException the leg converts — mapping a retriable outcome to Refused quarantines a safe row.
    public static DeliveryAck FromResult(PersistenceStatus status, string detail) => status switch {
        PersistenceStatus.Persisted => new Persisted(Duplicate: false),
        _                           => new Indeterminate(detail),
    };
}

// The shared sink row data every case carries once: the cursor-row key, the drain batch width, the held
// fencing token the cursor advance validates, and the bound delivery leg — the composition root fills `Leg`
// from the provider client (the blobstore `GrantMinter` idiom), so provider SDK types never enter a case.
public sealed record SinkBinding(SinkKey Key, int Batch, Option<LeaseToken> Held, Func<EgressSink, CloudEvent, OpLogEntry, IO<DeliveryAck>> Leg);

// The closed delivery-row family: every case carries its `SinkBinding` plus its transport lane as row DATA —
// a new sink is one case (its `Deliver` arm the only new code), zero pump edits. `Binding` and `Deliver`
// derive on the base through the generated total `Switch`, so the pump reads ONE surface over every row.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record EgressSink {
    private EgressSink() { }

    public sealed record Webhook(SinkBinding Bind, Uri Url, HashMap<string, string> Headers) : EgressSink;
    public sealed record Nats(SinkBinding Bind, string Subject) : EgressSink;
    public sealed record Kafka(SinkBinding Bind, string Topic, KafkaPublishMode Mode) : EgressSink;
    public sealed record RabbitMq(SinkBinding Bind, string Exchange, string RoutingKey) : EgressSink;
    public sealed record Pulsar(SinkBinding Bind, string Topic) : EgressSink;
    public sealed record WireNative(SinkBinding Bind, string HopKey) : EgressSink;
    public sealed record RedisStream(SinkBinding Bind, string Stream, string Group) : EgressSink;
    public sealed record ClickHouse(SinkBinding Bind, string Table) : EgressSink;

    public SinkBinding Binding => Switch(
        webhook: static w => w.Bind, nats: static n => n.Bind, kafka: static k => k.Bind,
        rabbitMq: static r => r.Bind, pulsar: static p => p.Bind, wireNative: static x => x.Bind,
        redisStream: static s => s.Bind, clickHouse: static c => c.Bind);

    // ONE delivery surface — the bound leg IS the case's provider composition (the Cases bullet spells every
    // leg: pg_net enqueue+reconcile, JetStream publish+Nats-Msg-Id, ToKafkaMessage+ProduceAsync under the
    // KafkaPublishMode row's Switch, confirm-awaited BasicPublishAsync, ISend.Send, the AppHost OutboundHop
    // hand-off, StreamAdd idempotent XADD, the ClickHouse async insert under insert_deduplication_token);
    // every leg converts its provider outcome to DeliveryAck at ITS boundary, never inside the pump.
    public IO<DeliveryAck> Deliver(CloudEvent envelope, OpLogEntry row) => Binding.Leg(this, envelope, row);
}

// --- [OPERATIONS] -----------------------------------------------------------------------

public static class Egress {
    // Declared once, set per event — typed extension attributes, never hand-built ce_ header strings.
    static readonly CloudEventAttribute TraceParent = CloudEventAttribute.CreateExtension("traceparent", CloudEventAttributeType.String);
    static readonly CloudEventAttribute Redacted = CloudEventAttribute.CreateExtension("redacted", CloudEventAttributeType.Boolean);

    // The ONE envelope rail: id = content key, source = the oplog URI, type = family.kind, partitionkey =
    // EntityKey RAW (the routing identity — redacting it collapses every partition to one value and destroys
    // per-entity order), sequence = the changefeed sequence. The PAYLOAD redacts BEFORE the envelope leaves
    // the trust boundary and the `redacted` attribute records that masking fired; the trace slot continues
    // as traceparent.
    public static CloudEvent Envelope(OpLogEntry row, Func<OpLogEntry, (ReadOnlyMemory<byte> Data, bool Redacted)> redact) {
        (ReadOnlyMemory<byte> data, bool masked) = redact(row);
        CloudEvent ce = new() {
            Id = row.ContentKey.ToString("x32"),
            Source = new Uri("rasm:persistence/oplog"),
            Type = $"rasm.oplog.{row.Family.Key}.{row.Kind.Key}",
            Time = row.Physical.ToDateTimeOffset(),
            DataContentType = "application/octet-stream",
            Data = data,
        };
        Partitioning.SetPartitionKey(ce, row.EntityKey);
        Sequence.SetSequence(ce, row.Sequence.ToString(System.Globalization.CultureInfo.InvariantCulture));
        if (masked) { ce[Redacted] = true; }
        row.Trace.Continue().IfSome(ctx => ce[TraceParent] = $"00-{ctx.TraceId}-{ctx.SpanId}-01");
        return ce;
    }
}
```

| [INDEX] | [SINK]       | [ADVANCE]                            | [REPLAY_ABSORBED_BY]                     |
| :-----: | :----------- | :----------------------------------- | :--------------------------------------- |
|  [01]   | webhook      | reconciled `request_status=SUCCESS`  | receiver idempotency-key                 |
|  [02]   | nats         | JetStream `PubAckResponse` Settle-ack | broker `Nats-Msg-Id` window              |
|  [03]   | kafka        | awaited broker persistence           | idempotent producer + consumer key dedup |
|  [04]   | rabbitmq     | awaited publisher confirm            | receiver CloudEvents `id` dedup          |
|  [05]   | pulsar       | `ISend.Send` → `MessageId`           | receiver CloudEvents `id` dedup          |
|  [06]   | wire-native  | `OutboundHop` honesty verdict        | AppHost hop policy                       |
|  [07]   | redis-stream | awaited `StreamAdd` id               | producer `StreamIdempotentId`            |
|  [08]   | clickhouse   | awaited async insert                 | `insert_deduplication_token`             |

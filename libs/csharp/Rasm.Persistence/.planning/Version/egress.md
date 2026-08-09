# [PERSISTENCE_VERSION_EGRESS]

`EgressPump` drains durable `OpLogEntry` rows past each sink cursor, projects one CloudEvents envelope, folds every provider result into `DeliveryAck`, and advances only a confirmed contiguous prefix. `EgressSink` closes the delivery targets and exposes their provider settings to the bound adapter. Content identity supplies the replay key; each sink row declares the mechanism that absorbs replay. Presence and awareness never enter this durable rail.

## [01]-[INDEX]

- [02]-[EGRESS_PUMP]: `EgressPump` drains one fold past each sink cursor — profile lane gate, advance law, dead-letter and replay rows, `EgressReceipt` floor, 8270 band.
- [03]-[EGRESS_SINK]: `Egress.Envelope` projects the CloudEvent every `EgressSink` row delivers, each outcome folding to `DeliveryAck` under the dedup, settlement-contract, and in-flight-bound columns.

## [02]-[EGRESS_PUMP]

- Owner: `EgressPump` the static surface owning the one drain bracket — notification wait, cursor read, windowed row drain, envelope projection, sink delivery, ack fold, cursor advance, dead-letter capture; `DeadLetterRow` the typed dead-letter document (content key, sink, sequence, fault, attempt count) stored in the SAME Marten session so a dead-letter and its cursor state commit atomically, its whole table the `Version/retention#RETENTION_CLASSES` `evidence` class and therefore one `Store/provisioning#SERVER_EXTENSIONS` `RollingWindow` row; `EgressReceipt` the per-drain evidence implementing the kernel `IValidityEvidence`; `EgressFault` the 8270 band; `EgressPorts` the injected delegate frame (`Wait` binds `NpgsqlConnection.WaitAsync` with its bounded poll, then feed, coordination, redaction, propagator trace-stamping, the selected `StoreProfile` both entries gate on, and the session-bound dead-letter triple — `Letters` reads, `DeadLetter` writes, `Retire` terminates) filled at the composition root.
- Entry: `EgressPump.Lane` spells this page's `Store/provisioning#SERVER_EXTENSIONS` `StoreProfile` lane token once, and BOTH drain entries open on `ports.Profile.Admits(EgressPump.Lane)` — a refusal is `EgressFault.LaneUnrealizable` returned before any wake, feed, delivery, or cursor read, because the embedded profile realizes NO egress lane and the absence belongs at this pump's own door rather than at a first publish against a store carrying no outbox relation at all; `public static StoreOptions Partition(StoreOptions opts)` publishes the `dead-letter` `Store/provisioning#SERVER_EXTENSIONS` `RollingWindow` mapping contribution the composition root folds over the spine seat; `public static IO<Fin<EgressReceipt>> Drain(EgressSink sink, OutboxCursor cursor, EgressPorts ports, ProjectionContext frame)` is the one pump — it drains `ReplayWindow.DurableOps(cursor.Sequence, sink.Binding.Batch)` rows from the changefeed (the `Version/ledger` windowed-read case parameterized for this drain — never a third read surface), projects each row through `Egress.Envelope`, delivers through the sink row's composition, folds every outcome to `DeliveryAck`, and advances the cursor through `Store/coordination` `OutboxAdvance(sink.Binding.Key, through)` ONLY past the contiguous `Persisted` prefix — the first `Indeterminate` holds the cursor at its predecessor (a held cursor re-drains, the sink's dedup absorbs the replay), a `Refused` writes the `DeadLetterRow` and the drain continues past it; `public static IO<Fin<EgressReceipt>> Replay(EgressSink sink, EgressPorts ports, ProjectionContext frame)` loads that sink's letters through `EgressPorts.Letters` at the sink's own batch width and re-delivers them by content key through the SAME envelope/deliver/ack fold — replay is the pump re-parameterized, never a second delivery path, and it takes the drain's own three arguments because the loader is the read half of the pair `DeadLetter`/`Retire` write.
- Auto: the drain is one fold per batch — `rows.Map(Egress.Envelope).Map(sink.Deliver)` folded left with the contiguous-prefix advance accumulator, so the drain hands rows to each leg in sequence order and a mid-batch refusal never advances past unconfirmed work; what a sink then preserves is its own engine's answer, not the envelope's — `partitionkey` reaches a real routing key on exactly two rows (Kafka `Message.Key`, Pulsar `MessageMetadata.Key`/`OrderingKey`), while NATS orders per subject, RabbitMQ per queue, MQTT per topic, and AMQP per link, none of which expose a key member at all, so per-entity order on those rows holds only where one entity's rows share one subject, queue, topic, or link (`#EGRESS_SINK`) and a blanket per-entity-order claim over the whole family reads as a guarantee six engines never made; the pump wakes on the coordination `pg_notify('rasm_outbox', sink)` channel through `NpgsqlConnection.WaitAsync` on an otherwise-idle connection, with the bounded poll as the correctness floor (a missed NOTIFY costs latency, never a lost row — the cursor law owns correctness); the webhook row's `DeliveryUnconfirmed` reconciliation re-reads `net._http_response` by request-id on the NEXT drain, so a PENDING response resolves without a dedicated poller; a crash between delivery and advance re-drains the suffix and every sink's dedup column states what absorbs it (`#EGRESS_SINK`); dead-letter replay decrements nothing — the receipt's conservation fold proves `delivered + held + deadLettered == drained` on every drain.
- Receipt: a drain rides `store.egress.drain` carrying the sink, the from/through sequences, and the delivered/duplicate/held/dead-lettered counts; a dead-letter rides `store.egress.deadletter` carrying the content key and the fault; a replay rides `store.egress.replay`; each settled drain receipt fires the `rasm.persistence.egress.delivered` observe point (`Store/observability#HOOK_RAIL`) as a composition-root tap on the drain outcome, never an emit call inside the fold.
- Packages: Npgsql (`NpgsqlConnection.Notification`/`WaitAsync` — the pump wake), Marten (`IDocumentSession.Store`/`SaveChangesAsync` — the dead-letter document; `StoreOptions.Schema.For<T>().PartitionOn` through `RollingWindow.Declare` — its rolling window), Rasm (`IValidityEvidence`/`ValidityClaim`), Microsoft.Extensions.Compliance.Redaction (`IRedactorProvider.GetRedactor(DataClassificationSet)` — the classified-field gate before the boundary), LanguageExt.Core, NodaTime, Thinktecture.Runtime.Extensions, BCL inbox.
- Growth: a new delivery target is one `EgressSink` case with its `Deliver` composition (`#EGRESS_SINK`) and one `outbox_cursor` row minted on first drain — zero pump edits; a new drain policy (batch width, wake channel, redaction set) is one `EgressPorts`/sink-row value; zero new surface — a per-sink pump, a second delivery path for replay, a fire-and-forget webhook, a presence row in the CDC drain, a lane gate seated at a caller instead of these two entries, or a CDC poller beside the changefeed is the deleted form because the pump is one fold, replay is the same fold, the advance law owns the cursor, and the durable lanes are the only drain source.
- Boundary: the pump drains the durable outbox — `Family.Durable` lanes past the per-sink cursor — and the presence/awareness lane (`durable: false`) NEVER enters the envelope (the lossy `DrainSurface` is its only transport); the cursor-advance CAS failure is `CoordinationFault.OutboxDrain` raised by the coordination store (the fenced write is its rail), while every delivery fault is THIS band — `DeadLetter` the poisoned entry, `SinkRefused` the sink-level refusal, `CursorStall` the held cursor evidence, `DeliveryUnconfirmed` the pg_net PENDING/ERROR unconfirmed state the advance law reconciles; classified fields redact BEFORE envelope construction (`ErasingRedactor` the fail-closed fallback) so an out-of-authority payload crosses masked, never raw; caller cancellation passes through untyped; the wire-native row hands bytes to the AppHost `OutboundHop` keyed pipeline and reads its delivery-honesty policy — Persistence never owns that channel; the letter table retires by partition drop, not by row sweep, so a letter neither `Retire` nor `Replay` ever consumed leaves at its window's trailing edge as one receipted `Version/retention#SWEEP_AND_GC` `DropPartition` and an unbounded letter table has no reachable state.

```csharp signature
// --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
using Rasm.Domain;                                // CorrelationId — the S0 causal half the frame seats
using Rasm.Persistence.Element;                   // FaultBand — the one band registry (graph#FAULT_TABLES)
using Rasm.Persistence.Store;                     // OutboxCursor / SinkKey / OutboxAdvance (coordination#OUTBOX_CURSOR); RollingWindow / StoreProfile (provisioning#SERVER_EXTENSIONS)
using System.Threading.Channels;                  // the AMQP leg's OWN in-flight bound — that client publishes none

namespace Rasm.Persistence.Version;

// --- [MODELS] ---------------------------------------------------------------------------

// `DeadLetterRow` stores as a Marten document in the SAME session as the cursor state, replayed by
// content key through the one pump fold. `Attempts` gates the replay schedule; `Fault` carries the refusing ack.
// `DeadLetterRow` admits under the `Version/retention#RETENTION_CLASSES` `evidence` class — a dead letter is refusal
// evidence carrying its ack, name-plus-epoch keyed and declared-expiry — and `Window` projects `At` onto the
// duplicated `DateTimeOffset` the `Store/provisioning#SERVER_EXTENSIONS` `RollingWindow` `dead-letter` row
// partitions on, one derived member and never a second stamp beside `At`. The whole table is that one class, so a
// letter no `Retire` ever consumed leaves with its partition as one receipted `DropPartition` rather than
// accumulating past every replay attempt the `Attempts` schedule admits.
public sealed record DeadLetterRow(UInt128 ContentKey, SinkKey Sink, long Sequence, string Fault, int Attempts, Instant At) {
    public Guid Id { get; init; } = Guid.CreateVersion7();
    public DateTimeOffset Window => At.ToDateTimeOffset();
}

// `EgressPorts` is the injected delegate frame the composition root fills: sink clients, the coordination advance arrow, the
// payload redactor, the dead-letter store/retire arrows, and the SELECTED `StoreProfile` — values on a
// Persistence-owned shape, never an AppHost type ([A.1]). `Profile` seats first because both entries read it
// before any other member, and it arrives as the deployment's own selected row rather than a boolean a caller
// computed, so the gate reads the same `Lanes` set the provisioning fold verified. `DeadLetter`/`Retire`/`Letters` close over the SAME Marten `IDocumentSession` as the
// cursor state (`Store` then `SaveChangesAsync` with the drain), so a letter and its drain commit atomically —
// `Letters` is the READ half that pairs them: a letter set is loaded by sink under the same tenant transaction
// that will retire or re-letter it, which is what makes `Replay` a closed fold rather than a surface whose
// caller has to find its own input and can therefore hand it letters from another sink or another tenant; `Redact`
// masks the row's classified payload fields (built from `IRedactorProvider.GetRedactor(DataClassificationSet)`,
// `ErasingRedactor` the fail-closed fallback) and reports whether masking fired. `Stamp` renders a continued
// `ActivityContext` onto the W3C pair at the propagator owning that format — the AppHost `TraceContext`
// carrier adapter — so version byte, sampled flag, and every tracestate mutation reach this envelope as
// injected values; a `$"00-{traceId}-{spanId}-01"` interpolation on this side re-mints the wire format the
// propagator owns and freezes both version and flag byte against a spec revision.
public sealed record EgressPorts(
    StoreProfile Profile,
    Func<IO<Unit>> Wait,
    Func<CoordinationOp, Option<LeaseToken>, IO<Fin<CoordinationReceipt>>> Coordinate,
    Func<ReplayWindow, IO<Seq<OpLogEntry>>> Feed,
    Func<OpLogEntry, (ReadOnlyMemory<byte> Data, bool Redacted)> Redact,
    Func<ActivityContext, (string Traceparent, Option<string> Tracestate)> Stamp,
    Func<SinkKey, int, IO<Seq<DeadLetterRow>>> Letters,
    Func<DeadLetterRow, IO<Unit>> DeadLetter,
    Func<DeadLetterRow, IO<Unit>> Retire);

// `EgressReceipt` rides the kernel validity floor as per-drain evidence: conservation is the fold — every drained row is
// delivered, held, or dead-lettered, exactly once ([C]).
public sealed record EgressReceipt(SinkKey Sink, long From, long Through, int Drained, int Delivered, int Duplicates, int Held, int DeadLettered, Duration Elapsed, Instant At, CorrelationId Correlation) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.Of(Through >= From),
        ValidityClaim.CountExactly(Delivered + Held + DeadLettered, Drained),
        ValidityClaim.CountAtLeast(Delivered, Duplicates));
}

// --- [ERRORS] ---------------------------------------------------------------------------
// Band 8270 (Element/graph#FAULT_TABLES registry row `Egress`): delivery faults ONLY — the failed cursor-CAS
// stays `CoordinationFault.OutboxDrain` in the fenced store's band. `DeliveryUnconfirmed` types the pg_net
// `net.request_status` PENDING/ERROR reconciliation state the advance law holds on, and `LaneUnrealizable` types
// that profile refusal — an admission verdict, not a delivery outcome — so it carries its lane token rather
// than a provider detail, and no drain state exists behind it to receipt.
[Union]
public abstract partial record EgressFault : Rasm.Domain.Expected, IValidationError<EgressFault> {
    private EgressFault() : base() { }

    public sealed record DeadLetter(UInt128 ContentKey, SinkKey Sink, string Detail) : EgressFault;
    public sealed record SinkRefused(SinkKey Sink, string Detail) : EgressFault;
    public sealed record CursorStall(SinkKey Sink, long Held) : EgressFault;
    public sealed record DeliveryUnconfirmed(SinkKey Sink, long RequestId) : EgressFault;
    public sealed record LaneUnrealizable(SinkKey Sink, string Lane) : EgressFault;

    public override int Code => FaultBand.Egress + Switch(
        deadLetter:          static _ => 1,
        sinkRefused:         static _ => 2,
        cursorStall:         static _ => 3,
        deliveryUnconfirmed: static _ => 4,
        laneUnrealizable:    static _ => 5);

    public override string Message => Switch(
        deadLetter:          static c => $"<dead-letter:{c.Sink.Value}:{c.ContentKey:x32}>:{c.Detail}",
        sinkRefused:         static c => $"<sink-refused:{c.Sink.Value}>:{c.Detail}",
        cursorStall:         static c => $"<cursor-stall:{c.Sink.Value}@{c.Held}>",
        deliveryUnconfirmed: static c => $"<delivery-unconfirmed:{c.Sink.Value}#{c.RequestId}>",
        laneUnrealizable:    static c => $"<lane-unrealizable:{c.Sink.Value}:{c.Lane}>");

    public override string Category => Switch(
        deadLetter:          static _ => "DeadLetter",
        sinkRefused:         static _ => "Sink",
        cursorStall:         static _ => "Cursor",
        deliveryUnconfirmed: static _ => "Unconfirmed",
        laneUnrealizable:    static _ => "Profile");

    public static EgressFault Create(string message) => new SinkRefused(SinkKey.Create("<none>"), message);
}

// --- [OPERATIONS] -----------------------------------------------------------------------

public static class EgressPump {
    public static readonly StoreSlot DrainSlot = StoreSlot.Create("store.egress.drain");
    public static readonly StoreSlot DeadLetterSlot = StoreSlot.Create("store.egress.deadletter");
    public static readonly StoreSlot ReplaySlot = StoreSlot.Create("store.egress.replay");
    public static readonly Seq<StoreSlot> Slots = Seq(DrainSlot, DeadLetterSlot, ReplaySlot);

    // Lane token this pump admits under, spelled to match its `Store/provisioning#SERVER_EXTENSIONS`
    // `StoreProfile.Lanes` member byte for byte — that set compares ORDINALLY, so a case or spacing drift here
    // reads as an absent lane and refuses every drain on a server profile that realizes it perfectly well.
    public const string Lane = "egress";

    // ONE refusal for both entries: an unrealizable lane is an admission verdict with no drain behind it, so it
    // answers before the wake and carries no from/through pair a caller could mistake for an empty window.
    static IO<Fin<EgressReceipt>> Unrealizable(EgressSink sink) =>
        IO.pure(Fin<EgressReceipt>.Fail(new EgressFault.LaneUnrealizable(sink.Binding.Key, Lane)));

    // `Partition` publishes the letter table's mapping contribution the composition root folds over the
    // `Element/graph#STREAM_GRAIN` spine seat: the policy is the `Store/provisioning#SERVER_EXTENSIONS` `RollingWindow` `dead-letter` row and
    // this publishes only the duplicated key, so a `PartitionOn` carrying its own period literals is the deleted
    // form. The contribution seats HERE because the S0 spine may not name this document type.
    public static StoreOptions Partition(StoreOptions opts) =>
        RollingWindow.DeadLetter.Declare<DeadLetterRow>(opts, static row => row.Window);

    // ONE drain fold: windowed rows -> envelope -> sink delivery -> DeliveryAck -> contiguous-prefix
    // cursor advance. The first Indeterminate HOLDS the cursor (re-drain; sink dedup absorbs the replay); a
    // Refused PERSISTS its DeadLetterRow through the session-bound port BEFORE counting, and the cursor
    // advances past it — the durable letter, not the cursor, owns the poisoned entry from that point.
    public static IO<Fin<EgressReceipt>> Drain(EgressSink sink, OutboxCursor cursor, EgressPorts ports, ProjectionContext frame) =>
        ports.Profile.Admits(Lane) ? Drained(sink, cursor, ports, frame) : Unrealizable(sink);

    static IO<Fin<EgressReceipt>> Drained(EgressSink sink, OutboxCursor cursor, EgressPorts ports, ProjectionContext frame) =>
        from mark in IO.lift(frame.Mark)
        from _ in ports.Wait()
        from rows in ports.Feed(ReplayWindow.DurableOps(cursor.Sequence, sink.Binding.Batch))
        from folded in rows.FoldM(
            (Through: cursor.Sequence, Delivered: 0, Duplicates: 0, Held: 0, Dead: 0, Open: true),
            (state, row) => !state.Open
                ? IO.pure(state with { Held = state.Held + 1 })
                : sink.Deliver(Egress.Envelope(row, ports.Redact, ports.Stamp), row).Bind(ack => ack.Switch(
                    persisted:     p  => IO.pure(state with { Through = row.Sequence, Delivered = state.Delivered + 1, Duplicates = state.Duplicates + (p.Duplicate ? 1 : 0) }),
                    indeterminate: _  => IO.pure(state with { Held = state.Held + 1, Open = false }),
                    // `Attempts: 1` is the MEASURED first attempt, not a filled slot: this fold is the only
                    // site a letter is minted at and the row it letters was delivered exactly once. Every
                    // later count comes from `Replay`'s own `Attempts + 1`, so the column is monotone from
                    // its first write and no arm publishes a count no delivery produced.
                    refused:       rf => ports.DeadLetter(new DeadLetterRow(row.ContentKey, sink.Binding.Key, row.Sequence, rf.Detail, Attempts: 1, frame.Now()))
                                             .Map(_ => state with { Through = row.Sequence, Dead = state.Dead + 1 })))).As()
        from advance in folded.Through > cursor.Sequence
            ? ports.Coordinate(new CoordinationOp.OutboxAdvance(sink.Binding.Key, folded.Through), sink.Binding.Held)
            : IO.pure(Fin<CoordinationReceipt>.Succ(default!))
        let receipt = new EgressReceipt(sink.Binding.Key, cursor.Sequence, folded.Through, rows.Count, folded.Delivered, folded.Duplicates, folded.Held, folded.Dead, frame.Elapsed(mark), frame.Now(), frame.Correlation)
        select advance.Match(Succ: _ => Fin<EgressReceipt>.Succ(receipt), Fail: error => Fin<EgressReceipt>.Fail(error));

    // Replay IS the drain fold re-parameterized over the letter set — never a second delivery path. The letter
    // set is READ here through `ports.Letters` at the sink's own batch width rather than handed in, so replay
    // takes the same three arguments the drain takes and no caller can pair a sink with another sink's or
    // another tenant's letters. Each letter re-reads its row through the ONE windowed feed (the singleton
    // window at Sequence-1), re-delivers through the same envelope/leg, and a Persisted retires the letter; a
    // still-refusing row re-letters with Attempts+1 — attempts are MONOTONE by construction, the count is the
    // replay schedule's gate, and `Retire` is the one terminal, so no reset arrow exists to fabricate a fresh
    // budget for a poison row; a vanished row (retention-swept) retires as Held — the conservation fold closes
    // over letters exactly as the drain closes over rows.
    public static IO<Fin<EgressReceipt>> Replay(EgressSink sink, EgressPorts ports, ProjectionContext frame) =>
        ports.Profile.Admits(Lane) ? Replayed(sink, ports, frame) : Unrealizable(sink);

    static IO<Fin<EgressReceipt>> Replayed(EgressSink sink, EgressPorts ports, ProjectionContext frame) =>
        from mark in IO.lift(frame.Mark)
        from letters in ports.Letters(sink.Binding.Key, sink.Binding.Batch)
        from folded in letters.FoldM(
            (Delivered: 0, Duplicates: 0, Held: 0, Dead: 0),
            (state, letter) =>
                from rows in ports.Feed(ReplayWindow.DurableOps(letter.Sequence - 1, 1))
                let found = rows.Filter(r => r.ContentKey == letter.ContentKey).Head
                from next in found.Match(
                    Some: row => from ack in sink.Deliver(Egress.Envelope(row, ports.Redact, ports.Stamp), row)
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
|   [01]  | drain source  | `ReplayWindow.DurableOps` past the sink cursor | one windowed read (ledger); presence never enters                |
|   [02]  | advance law   | contiguous `Persisted` prefix only             | Indeterminate holds; Refused dead-letters and continues          |
|   [03]  | replay        | `Letters` loads, the same fold re-delivers     | monotone `Attempts`; `Retire` terminal; no second delivery path  |
|   [04]  | wake          | `WaitAsync` on `rasm_outbox` + bounded poll    | NOTIFY is latency; the poll floor owns correctness               |
|   [05]  | redaction     | `Redact` before envelope construction          | fail-closed `ErasingRedactor`; classified fields never cross raw |
|   [06]  | receipt floor | conservation `ValidityClaim.All` fold          | delivered + held + dead == drained, exactly once ([C])           |

## [03]-[EGRESS_SINK]

- Owner: `Egress.Envelope` the one CloudEvents projection of an `OpLogEntry` — populating required `Id` (the content key, lower-hex), `Source` (`rasm:persistence/oplog`), `Type` (`rasm.oplog.{family}.{kind}`), `Time` (the entry's `Physical` instant), `Data` (the redacted payload bytes, `application/octet-stream`), `Partitioning.SetPartitionKey(EntityKey)`, `Sequence.SetSequence(entry.Sequence.ToString(InvariantCulture))`, and the `traceparent`/`tracestate`/`redacted` extension attributes declared ONCE via `CloudEventAttribute.CreateExtension` — one envelope for every sink, one shared `JsonEventFormatter` whose `JsonSerializerOptions` compose `ConfigureForNodaTime(DateTimeZoneProviders.Tzdb)` + `ThinktectureJsonConverterFactory` (disjoint type-spaces, co-registered once at construction) and pin `AllowDuplicateProperties = false` — the shared codec serves the `Version/ingress` decode of FOREIGN broker payloads, so a duplicate-name envelope REFUSES at read rather than last-write-winning a hostile attribute; `EgressSink` the closed `[Union]` delivery-row family whose every case carries its dedup stance, batch width, held token, and out-of-band fault reader as row DATA; `DeliveryAck` the one `[Union]` every sink outcome folds to at the sink's own boundary — a raw `PubAckResponse`/`DeliveryResult`/`MessageId` never crosses into the pump; `SinkBinding.Watch` the one cell every leg consults before folding, because five of these engines publish a delivery failure on a surface the awaited return cannot reach and a leg reading only its await is blind to exactly the faults that matter.
- Cases: `Webhook(Uri Url, HashMap<string, string> Headers)` — enqueue `net.http_post(url, body, headers + idempotency-key = ContentKey-hex)` returning the `bigint` request-id, the `application/cloudevents+json` body page-minted through `Egress.StructuredBody` (the streamed encode — a blob-bearing op enqueues with no contiguous base64 materialization), fold `net.http_response_result` on the NEXT drain: `SUCCESS` → `Persisted`, `PENDING` → `Indeterminate` (`DeliveryUnconfirmed` evidence), `ERROR`/timeout → `Refused`; a pg_net UNLOGGED-table crash loses response rows, so the held cursor re-posts under the SAME idempotency-key header — receiver-side dedup, the row's honest stance. `Nats(string Subject)` — `INatsJSContext.Publish` with `NatsHeaders["Nats-Msg-Id"] = ContentKey-hex` beside the `traceparent`/`tracestate`/`baggage` header rows the AppHost `TraceContext` carrier adapter stamps, so every JetStream delivery joins the drain trace exactly as the Kafka leg's instrumented producer does; the `TryPublishAsync` ROP form answers `NatsResult<PubAckResponse>` and the fold reads BOTH of its error surfaces, because they mean different things and only one of them is a transport fault: `NatsResult.Error` is the publish never reaching the stream (`Indeterminate` — the held cursor re-drives into the dedup window), while `PubAckResponse.Error` rides a SUCCESSFUL result and is the stream's own rejection (`Refused`), so folding the result alone reports `Persisted` for an ack the stream refused; `PubAckResponse.Duplicate` is the dedup-window replay absorbed (`Persisted(Duplicate: true)`, `api-nats` JETSTREAM_DELIVERY_ACK). Batch drains pipeline through `PublishConcurrentAsync` and settles each `NatsJSPublishConcurrentFuture.GetResponseAsync` in offer order, so the window's round trips overlap while the contiguous-prefix advance still reads them sequentially — the one publish shape where this client's ack is decoupled from its send. `Kafka(string Topic, KafkaPublishMode Mode)` — `cloudEvent.ToKafkaMessage(ContentMode.Binary, formatter)` (attributes ride `ce_*` headers, `partitionkey` projects onto `Message.Key`, the ONE key member that makes per-entity order real here); the leg's `ProducerConfig` pins `EnableIdempotence = true` with `Acks.All`, because the dedup column claims broker-side suppression of producer retries and an unset flag leaves that claim unconfigured; the producer builds through `AsInstrumentedProducerBuilder` under `ConfluentKafkaInstrumentedProducerBuilderOptions` (`EnableTraces`/`EnableMetrics`), so delivery spans and client meters continue the envelope `traceparent` from drain to broker ack; `Awaited` folds the awaited `ProduceAsync` broker ack (`Status == Persisted` → `Persisted`, `NotPersisted`/`PossiblyPersisted` → `Indeterminate`) and a caught `ProduceException` through `DeliveryAck.FromError` on `Error.IsFatal` — `Error.IsRetriable` is INTERNAL, so `IsFatal` is the client's only public discriminator and a non-fatal produce fault re-drives rather than dead-letters; `ReadCommitted` brackets the publish with `InitTransactions` → `BeginTransaction` → `CommitTransaction` so `isolation.level=read_committed` consumers never observe an aborted record, and the pump's own close drains through `Flush(TimeSpan)` so a bounded shutdown lands what the produce queue already holds. Kafka transactions cannot commit the PostgreSQL outbox cursor; every mode remains at-least-once across that boundary and content-key consumer dedup absorbs a crash after broker persistence but before cursor advance. `RabbitMq(string Exchange, string RoutingKey)` — `CreateChannelOptions(publisherConfirmationsEnabled: true, publisherConfirmationTrackingEnabled: true)` then awaited `BasicPublishAsync(exchange, routingKey, mandatory: true, properties, body, token)` (the await IS the confirm, and a nack throws under tracking): completion → `Persisted`, a caught nack → `Refused`. `mandatory: true` is load-bearing rather than decorative — an unroutable message under `mandatory: false` is discarded by the broker while the confirm still ACKS, so the leg reports `Persisted` for a row that reached no queue; the `BasicReturnEventArgs` the flag unlocks rides the row's `Watch` cell. `BasicProperties.DeliveryMode = DeliveryModes.Persistent` pins the message durable, since a durable quorum queue still loses a `Transient` message on broker restart and the queue's own durability never covers its contents. `Pulsar(string Topic)` — `ISend.Send(metadata, payload)` → `MessageId` (`LedgerId:EntryId:Partition:BatchIndex`, the whole receipt: this client publishes NO status enum beside it) → `Persisted`; `MessageMetadata.SequenceId` carries the content key's low 64 bits as the broker's own dedup input (a custom property is a payload the broker never deduplicates on), `MessageMetadata.Key` carries `partitionkey` for routing, and `OrderingKey` carries the same `EntityKey` bytes so a `KeyShared` subscription keeps per-entity order across a rebalance; the producer builds under `ProducerAccessMode.WaitForExclusive` so one WAL-leader producer per topic is elected and a loser's `ProducerFencedException` re-elects rather than double-writes, and its `ISchema<T>` is the required `Schema.ByteArray` over the binary envelope — a producer built with no schema is refused by the client. `WireNative(string HopKey)` — Persistence writes `MessageExtensions.WriteLengthPrefixedTo` bytes onto the AppHost `OutboundHop` keyed pipeline and folds the hop's delivery-honesty verdict; the gRPC channel is AppHost-owned. `RedisStream(string Stream, string Group)` — await `StreamAdd(stream, fields, StreamIdempotentId(ContentKey-hex), trimMode: StreamTrimMode.Acknowledged)`; a returned stream id is `Persisted`, and a transport ambiguity is `Indeterminate`. Downstream consumers own `StreamReadGroup`/`StreamAcknowledge`; their independent group cursor never governs the PostgreSQL outbox cursor. `Amqp(string Address)` — the `AMQP 1.0` native binding DISTINCT from the 0-9-1 `RabbitMq` case (the two protocols share no message type, so a re-bind of the `RabbitMq` `Deliver` leg through this binding is structurally impossible and the transport family closes as two rows over the one envelope): `cloudEvent.ToAmqpMessageWithUnderscorePrefix(ContentMode.Binary, formatter)` maps the envelope onto AMQPNetLite's `Amqp.Message` with `cloudEvents_type`/`cloudEvents_source` application properties a header-filtering broker routes on, delivered through awaited `SenderLink.SendAsync(message, timeout)` inside the row's OWN bounded in-flight window — settled disposition → `Persisted`, transport ambiguity → `Indeterminate`, broker rejection → `Refused` (`AmqpException` over a `Released`/`Rejected` outcome, the awaited form raising it rather than reporting a status value); replay absorbs receiver-side on the CloudEvents `id`. That window is the `[BOUND]` column's whole AMQP value BECAUSE the client publishes no sender-side credit member: the PEER grants credit on its `Flow`, `ReceiverLink.SetCredit` has no `SenderLink` counterpart, and the callback form `Send(message, callback, state)` appends to an unbounded internal list the moment credit is absent, growing managed memory no fence member can read or cap — so that callback form is the DELETED form here and the awaited pair is the only admitted send. `InFlight` sizes a bounded `Channel` under `BoundedChannelFullMode.Wait`, offered in drain order and settled in OFFER order exactly as the NATS `PublishConcurrentAsync` row settles its futures, so the window's round trips overlap while the contiguous-prefix advance still reads them sequentially; `Wait` is the one admissible full mode because every drop mode discards a durable row the `EgressReceipt` conservation fold carries no arm to express, and the doctrine's loss column is empty for exactly that reason. This binding EXCLUDES `datacontenttype` from the application-property map and folds it into `Properties.ContentType` instead, so a broker header-filter selects on `cloudEvents_type`/`cloudEvents_source` and never on content type; `Uri` attributes serialize through `ToString()` and timestamps through `UtcDateTime`, which is why the envelope's `Time` crosses as UTC with its offset dropped and a consumer reconstructing local wall time reads the stamp, never the property. `ClickHouse(string Table)` (the table IS `Query/columnar#ANALYTICS_RESIDENCE` `WarehouseSchema.Table` with its `WarehouseSchema.Columns` roster — writer and reader share ONE typed row vocabulary, never two independently-authored shapes) — `ClickHouseClient.InsertBinaryAsync` (the pooled RowBinary ingest rail) with the `insert_deduplication_token` server setting = ContentKey-hex riding the `InsertOptions`/`QueryOptions` custom-settings row (the producer-supplied dedup id ClickHouse deduplicates replays on — exactly the content-key dedup stance the envelope law demands; `BeginDbTransaction` throws `NotSupportedException`, so the token IS the sink's whole dedup story), the awaited insert completion → `Persisted`, a transient server/connection fault → `Indeterminate` (the held cursor re-drives under the SAME token, absorbed), a schema/table rejection → `Refused`; the billion-row fleet-analytics lane whose read side is the `Query/columnar#ANALYTICS_RESIDENCE` `Residence.Fleet` row. Its `Watch` cell is the one row on this family standing behind NO event: this driver declares zero events on its connection, never raises the inherited `DbConnection.StateChange` (its backing state field takes a bare write on open and on close, and `OnStateChange` is called nowhere in the assembly), and ships no pool type at all — `ClickHouseConnectionFactory` is a `DbProviderFactory`, never a pool — so `ClickHouseConnection.State` echoes only an explicit `Open`/`Close` this fence already made and reading it answers nothing about the server. That cell is fed by an ACTIVE probe instead: the composition root runs `ClickHouseClient.PingAsync(QueryOptions?, CancellationToken)` on its own cadence and folds a false answer or a thrown `ClickHouseServerException` into the same cell every event-bearing row writes, so the `Watch` arrow holds ONE shape across the whole family and only this row's producer differs. `ClickHouse.Driver.Diagnostic.ClickHouseDiagnosticsOptions` and `TraceHelper` carry the `ActivitySource` name, the SQL-capture gate, and a `System.Net` trace toggle — telemetry configuration, never a fault signal — so neither feeds the cell; `ClickHouseBulkCopy.BatchSent` is the assembly's ONE event and stays declined, because it rides an `[Obsolete]` type this leg never constructs, its `RowsWritten` reports a cumulative row total rather than a per-envelope disposition, and the admitted `InsertBinaryAsync` rail publishes no progress callback beside it — settlement stays the awaited completion. `Mqtt(string Topic)` — `cloudEvent.ToMqttApplicationMessage(ContentMode.Structured, formatter, topic)` (the `CloudNative.CloudEvents.Mqtt` binding is structured-mode ONLY — the whole envelope rides the payload body under `application/cloudevents+json`, so there is no `ce_*` header-route form and a broker filters on the topic string alone; a `ContentMode.Binary` call is a compile-legal run-time `ArgumentOutOfRangeException` the leg never makes), and the returned `MqttApplicationMessage` is the EXACT object `IMqttClient.Publish` consumes with zero re-map; the per-sink `IMqttClient` is `MqttClientFactory.CreateMqttClient()`-minted (per-instance, disposed with the sink — no host-wide singleton) under `MqttClientOptionsBuilder.WithProtocolVersion(MqttProtocolVersion.V500)` so the v5 `UserProperties` carrier is live (every v5 field drops SILENTLY under `V311` — no throw, no reason code, so the version pin is what keeps the trace pair and the expiry from vanishing), `.WithRequestProblemInformation(true)` so a refusing broker returns the `ReasonString` the `Refused` detail carries (without it the reason string is absent and every refusal dead-letters with a bare code), and `.WithCleanStart(false)` under a non-zero `.WithSessionExpiryInterval(seconds)` so in-flight QoS-1 state survives a reconnect — the ONLY resume this protocol carries, since it publishes no offset and no sequence; the encode carries `MqttApplicationMessageBuilder.WithMessageExpiryInterval(seconds)` so a broker drops an undeliverable envelope at its own edge rather than holding it past the window the letter table already covers, and the leg stamps the `traceparent`/`tracestate` pair onto `MqttApplicationMessage.UserProperties` (`List<MqttUserProperty>`, through the `ReadOnlyMemory<byte>` `ValueBuffer` overload — the string `WithUserProperty` is `[Obsolete]`) beside the encode exactly as the NATS-header and AMQP-application-property legs do; the awaited `PublishAsync` returns `MqttClientPublishResult` whose `IsSuccess` (`MqttClientPublishReasonCode.Success` or `NoMatchingSubscribers`, a delivered-but-unrouted success and never a fault) → `Persisted`, a transport ambiguity (client disconnect, timeout) → `Indeterminate` (the held cursor re-drives and receiver-side dedup on the CloudEvents `id` absorbs the replay), and a definitive `128`+ reason code (`NotAuthorized`, `TopicNameInvalid`, `QuotaExceeded`, `PayloadFormatInvalid`) → `Refused` with its reason string. This client never throws on the wire at all — connect, publish, subscribe, and unsubscribe each return their reason code as a VALUE and its disconnect cause is a field on `MqttClientDisconnectedEventArgs`, so `128`+ on the returned result is the whole fault vocabulary and the disconnect event is what the row's `Watch` cell reads; a leg catching exceptions here catches only the builder's construction-time `MqttProtocolViolationException`, which never crosses the boundary.
- Entry: `public IO<DeliveryAck> Deliver(CloudEvent envelope, OpLogEntry row)` resolves to the case's bound `SinkBinding.Leg` (each leg is ONE conversion site folding its provider outcome to `DeliveryAck`, filled at the composition root from the provider client), and `public SinkBinding Binding` derives the shared `(Key, Batch, Held)` row data the pump and the cursor read.
- Auto: refusal SHAPE is a column too, and it is the one every naive adapter gets wrong — `throw` alone covers redis and clickhouse, MQTT is `value` only (its publish never throws on the wire), Pulsar adds reactive `IState`, RabbitMQ and NATS and Kafka add events and callbacks, so the family reads its awaited return AND the row's `Watch` cell rather than assuming one rail every engine shares; the webhook row's multi-row drain encodes ONE `application/cloudevents-batch` body through `CloudEventFormatter.EncodeBatchModeMessage(IEnumerable<CloudEvent>, out ContentType)` only under a `WebhookSettle.PerEnvelope` receiving contract that also advertises the batch media type (`MimeUtilities.IsCloudEventsBatchContentType`), never on the transport's own say-so — `net.http_post` hands back one `bigint` id, `net._http_response` stores ONE `status_code` against that id, and the CloudEvents batch binding defines no per-event response element, so a receiver settling per REQUEST answers N envelopes with a single status and the drain reports one merged tally where `EgressReceipt` carries `Delivered` beside `Duplicates` as separate halves, a tally that cannot tell zero redelivery from a wedged retry and states a duplicate count no response ever produced; `PerRequest` is therefore the pg_net floor and its cursor-advancing drain posts SINGLE-row bodies through the streamed structured encode, while `PerEnvelope` names a receiver publishing a per-envelope disposition its `Read` arrow folds into one `DeliveryAck` per envelope in offer order — which is what makes a batched round trip honest rather than merely cheap, the batch encoder's contiguous materialization being the second reason the size-bearing single-row path stays; dedup honesty is a COLUMN, not prose — every case states what absorbs a replay: NATS the broker dedup window on `Nats-Msg-Id`, Kafka the idempotent producer and content-key consumer dedup, webhook/pulsar/wire-native/amqp/mqtt receiver-side id-dedup on the CloudEvents `id`, redis the producer-side `StreamIdempotentId`, clickhouse the producer-side `insert_deduplication_token`; the envelope is `ContentMode.Binary` everywhere a header-bearing transport carries a binary CloudEvents form so a broker filters on `ce_type`/`ce_source`/`partitionkey` without parsing the body, and MQTT is the structured-mode exception — the CloudEvents MQTT binding has no binary content mode, so the whole envelope rides the payload body under `application/cloudevents+json` and its broker filters on the topic string alone; serdes-governed Kafka bodies own the `Data` bytes and their schema-id framing beside the `ce_*` envelope headers with zero key collision — the composition root's payload arrow frames them through `AvroSerializer<T>`/`JsonSerializer<T>`/`ProtobufSerializer<T>` `SerializeAsync` over one `CachedSchemaRegistryClient` under `SchemaRegistryConfig`, so registry framing precedes the envelope and envelope codec and body codec never share a `JsonSerializerOptions`.
- Receipt: per-sink delivery evidence rides the drain receipt (`#EGRESS_PUMP`); the sink names its lane through `Binding`, never a free string.
- Packages: CloudNative.CloudEvents (+`.SystemTextJson` `JsonEventFormatter`, `CloudEventFormatter.EncodeBatchModeMessage` + `MimeUtilities.IsCloudEventsBatchContentType` — the batched webhook body, +`.Kafka` `ToKafkaMessage`, +`.Amqp` `AmqpExtensions.ToAmqpMessageWithUnderscorePrefix`/`ToCloudEvent` over the AMQPNetLite `Amqp.Message` carrier — the `AMQP 1.0` leg, protocol-disjoint from `RabbitMQ.Client`, +`.Mqtt` `MqttExtensions.ToMqttApplicationMessage` structured-mode `CloudEvent` → `MqttApplicationMessage` over the MQTTnet carrier — the MQTT v5 leg, payload-body-only; `Partitioning`/`Sequence` extension attributes), NATS.Net (`INatsJSContext.Publish`/`TryPublishAsync`/`PublishConcurrentAsync` + `NatsJSPublishConcurrentFuture.GetResponseAsync`, `NatsHeaders`, `NatsResult<T>.Error`, `PubAckResponse.Duplicate`/`.Error`), Confluent.Kafka (`ProduceAsync`, `DeliveryResult.Status`, `ProducerConfig.EnableIdempotence` + `Acks.All`, `ProduceException`/`Error.IsFatal`, `IProducer.Flush(TimeSpan)`, `InitTransactions`/`BeginTransaction`/`CommitTransaction`), Confluent.SchemaRegistry (`CachedSchemaRegistryClient`/`SchemaRegistryConfig`) + serdes (`AvroSerializer<T>`/`JsonSerializer<T>`/`ProtobufSerializer<T>` `SerializeAsync` — the composition-root payload framing), OpenTelemetry.Instrumentation.ConfluentKafka (`AsInstrumentedProducerBuilder` + `ConfluentKafkaInstrumentedProducerBuilderOptions` at the leg's builder seam; `AddKafkaProducerInstrumentation` registers on the tracer and meter builders at the AppHost root), RabbitMQ.Client (`CreateChannelOptions` confirms, `BasicPublishAsync(exchange, routingKey, mandatory, properties, body, token)`, `BasicProperties.DeliveryMode`/`DeliveryModes.Persistent`, `BasicReturnEventArgs`), MQTTnet (`MqttClientFactory.CreateMqttClient` per-instance mint, `IMqttClient.Publish` → `MqttClientPublishResult`, `MqttApplicationMessageBuilder`, `MqttClientOptionsBuilder.WithProtocolVersion(V500)`/`.WithRequestProblemInformation`/`.WithCleanStart`/`.WithSessionExpiryInterval`, `MqttApplicationMessageBuilder.WithMessageExpiryInterval`, `MqttUserProperty.ValueBuffer` — the QoS-1 PUBACK reason-code leg and the v5 UserProperties tracing carrier), DotPulsar (`ISend.Send` → `MessageId`, `MessageMetadata.SequenceId`/`.Key`/`.OrderingKey`, `ProducerAccessMode.WaitForExclusive`, `ProducerState`/`IState`, `Schema.ByteArray`), StackExchange.Redis (`StreamAdd`/`StreamIdempotentId`/`StreamTrimMode.Acknowledged`), AMQPNetLite.Core (`Amqp.SenderLink.SendAsync(Message, TimeSpan)` — the awaited send whose ack IS the settlement; `Amqp.Message`; `Amqp.AmqpException.Error`; `Amqp.AmqpObject.Closed`/`AddClosedCallback` — the link-and-connection fault the `Watch` cell reads; `Amqp.Session`/`Amqp.Connection` — the link's owning pair), System.Threading.Channels (`Channel.CreateBounded<T>(BoundedChannelOptions)`, `BoundedChannelOptions(int)` under `BoundedChannelFullMode.Wait`, `ChannelWriter<T>.WriteAsync`, `ChannelReader<T>.ReadAllAsync` — the AMQP leg's own in-flight window, since that client bounds nothing), ClickHouse.Driver (`ClickHouseClient.InsertBinaryAsync` + `InsertOptions`/`QueryOptions` custom settings — the warehouse leg under the `insert_deduplication_token` producer-supplied dedup setting; `ClickHouseClient.PingAsync` — the cadence probe feeding this row's `Watch` cell, the one driver here publishing no event), pg_net (`net.http_post`/`net.request_status`/`net.http_response_result` over raw Npgsql), Google.Protobuf (`WriteLengthPrefixedTo` — the wire-native payload), NodaTime.Serialization.SystemTextJson (`ConfigureForNodaTime` on the one formatter options), Thinktecture.Runtime.Extensions.Json (`ThinktectureJsonConverterFactory` co-registered; the options pin `AllowDuplicateProperties = false` for the shared decode leg), BCL inbox (`Utf8JsonWriter.WriteBase64StringSegment`/`.BytesPending` — the streamed structured-body encode).
- Growth: a new delivery target is ONE `EgressSink` case carrying its `Deliver` composition and dedup column — the pump, the envelope, the cursor, and the receipt are untouched; a new envelope attribute is one `CloudEventAttribute.CreateExtension` declaration; a new receiving contract is one `WebhookSettle` case and a new in-flight ceiling one `Amqp` row value; zero new surface — a per-sink envelope shape, a hand-built `ce_` header, a raw provider ack crossing into the pump, a second formatter, a fire-and-forget publish on a durable row, an unawaited AMQP callback send, a batched body under a per-request contract, or a connection-state read standing in for a `Watch` cell is the deleted form.
- Boundary: the envelope is the single cross-consumer, cross-language vocabulary — the AppHost outbox relay and the durable-orchestration dispatch drain the SAME CloudEvents projection as their hop payload, so a per-consumer re-pack is the drift defect; `id` is the content key so replay dedup is content identity, never a broker sequence; the webhook row NEVER fire-and-forgets — `net.http_post` enqueues and the response reconciliation is the only advance authority; its structured body streams through `Egress.StructuredBody` — one contiguous base64 materialization of a blob-bearing op is the deleted form, and the segment writer holds 3-byte pages so the emitted string is byte-identical to the formatter's; the wire-native row reads the AppHost delivery-honesty policy (the database is excluded from the AppHost hop law; sink delivery is not); the redis row's `StreamTrimMode.Acknowledged` trim keeps the stream bounded by consumption, never a time guess; the sink family is egress-only — the inbound Kafka consume leg is the `Version/ingress` `CdcIngress` owner where the consumer-side instrumented twins bind, never a sink case, and its content-key dedup is the consumer half every dedup-honesty row here presumes.

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

// Receiving-CONTRACT column deciding whether one request may carry a whole drain window. `PerRequest` is the
// pg_net floor — one enqueued request answers with one stored status, and the CloudEvents batch binding names no
// per-event response element, so a batched body under it fills `Delivered` and `Duplicates` from a single number
// and publishes a duplicate count nothing measured. `PerEnvelope` admits the batch encode because the RECEIVER
// publishes a disposition per envelope; `Read` folds that body into one ack per envelope IN OFFER ORDER, which
// is the order the contiguous-prefix advance consumes them in, and a short or reordered answer refuses on the
// `Fin` rail rather than silently pairing an ack with the wrong row.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record WebhookSettle {
    private WebhookSettle() { }

    public sealed record PerRequest : WebhookSettle;
    public sealed record PerEnvelope(Func<string, int, Fin<Seq<DeliveryAck>>> Read) : WebhookSettle;
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

    // Thrown-side twin of the same fold. `Error.IsRetriable` is INTERNAL on this client, so `IsFatal` is the one
    // public discriminator a leg can read: a non-fatal produce fault (leader election, transient broker refusal)
    // re-drives under the held cursor, and only a fatal one earns the letter table. Collapsing every caught
    // ProduceException onto Refused quarantines rows the very next attempt would have taken.
    public static DeliveryAck FromError(Error error) =>
        error.IsFatal ? new Refused(error.Reason) : new Indeterminate(error.Reason);
}

// `SinkBinding` is the shared sink row data every case carries once: the cursor-row key, the drain batch width, the held
// fencing token the cursor advance validates, the bound delivery leg, and the out-of-band fault reader — the
// composition root fills `Leg` from the provider client (the blobstore `GrantMinter` idiom), so provider SDK types
// never enter a case.
//
// `Watch` closes the result-shaped rail trap at the FAMILY owner rather than per provider. Six transports report a
// delivery failure somewhere the awaited return cannot reach: Pulsar publishes `ProducerState.Faulted`/`Fenced` on a
// reactive `IState` await, RabbitMQ delivers an unroutable `BasicReturnEventArgs` and every connection fault on an
// event, Kafka hands `Error` to `SetErrorHandler`, NATS raises `MessageDropped`/`SlowConsumerDetected`/`ReconnectFailed`
// as events, MQTT carries its disconnect cause as a FIELD on `MqttClientDisconnectedEventArgs`, and every AMQP object
// — link and connection alike — raises `AmqpObject.Closed` (or `AddClosedCallback`) carrying its `Error`, so a link
// detached by the peer reports through this cell rather than through a send that already returned. Each provider's own
// surface subscribes into one cell at the composition root and this arrow reads it, so a leg folding only what its
// await returned can no longer report `Persisted` for a row the transport already told someone else it lost.
// ClickHouse is the lone row with NO event to subscribe: its cell is fed by a cadence `PingAsync` probe instead,
// which is why this arrow stays a plain cell read and never grows a per-provider subscription shape.
public sealed record SinkBinding(
    SinkKey Key,
    int Batch,
    Option<LeaseToken> Held,
    Func<EgressSink, CloudEvent, OpLogEntry, IO<DeliveryAck>> Leg,
    Func<Option<string>> Watch);

// Closed delivery-row family: every case carries its `SinkBinding` and its transport lane as row DATA —
// a new sink is one case (its `Deliver` arm the only new code), zero pump edits. `Binding` and `Deliver`
// derive on the base through the generated total `Switch`, so the pump reads ONE surface over every row.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record EgressSink {
    private EgressSink() { }

    public sealed record Webhook(SinkBinding Bind, Uri Url, HashMap<string, string> Headers, WebhookSettle Settle) : EgressSink;
    public sealed record Nats(SinkBinding Bind, string Subject) : EgressSink;
    public sealed record Kafka(SinkBinding Bind, string Topic, KafkaPublishMode Mode) : EgressSink;
    public sealed record RabbitMq(SinkBinding Bind, string Exchange, string RoutingKey) : EgressSink;
    public sealed record Amqp(SinkBinding Bind, string Address, int InFlight) : EgressSink;
    public sealed record Pulsar(SinkBinding Bind, string Topic) : EgressSink;
    public sealed record WireNative(SinkBinding Bind, string HopKey) : EgressSink;
    public sealed record RedisStream(SinkBinding Bind, string Stream, string Group) : EgressSink;
    public sealed record ClickHouse(SinkBinding Bind, string Table) : EgressSink;
    public sealed record Mqtt(SinkBinding Bind, string Topic) : EgressSink;

    public SinkBinding Binding => Switch(
        webhook: static w => w.Bind, nats: static n => n.Bind, kafka: static k => k.Bind,
        rabbitMq: static r => r.Bind, amqp: static a => a.Bind,
        pulsar: static p => p.Bind, wireNative: static x => x.Bind,
        redisStream: static s => s.Bind, clickHouse: static c => c.Bind, mqtt: static m => m.Bind);

    // ONE delivery surface — the bound leg IS the case's provider composition (the Cases bullet spells every
    // leg: pg_net enqueue+reconcile, JetStream publish+Nats-Msg-Id, ToKafkaMessage+ProduceAsync under the
    // KafkaPublishMode row's Switch, confirm-awaited BasicPublishAsync, SendAsync inside the AMQP row's own
    // bounded in-flight window, ISend.Send, the AppHost OutboundHop hand-off, StreamAdd idempotent XADD, the
    // ClickHouse async insert under insert_deduplication_token, the ToMqttApplicationMessage structured encode
    // + PublishAsync PUBACK reason-code fold);
    // every leg converts its provider outcome to DeliveryAck at ITS boundary, never inside the pump.
    //
    // Watch reads the row's out-of-band cell AFTER the leg settles and downgrades a Persisted alone: an
    // acknowledged send standing beside a pending transport fault is exactly the ambiguous case, so the cursor
    // HOLDS and the row re-drives into the sink's own dedup rather than advancing past a row the transport
    // reported lost on a surface the await never saw. Indeterminate and Refused already carry their own detail
    // and take no second one; a clean cell is the common path and costs one Option read.
    public IO<DeliveryAck> Deliver(CloudEvent envelope, OpLogEntry row) =>
        Binding.Leg(this, envelope, row).Map(ack => (ack, Binding.Watch()) switch {
            (DeliveryAck.Persisted, { IsSome: true, Case: string pending }) => new DeliveryAck.Indeterminate(pending),
            _                                                              => ack,
        });
}

// --- [OPERATIONS] -----------------------------------------------------------------------

public static class Egress {
    // Declared once, set per event — typed extension attributes, never hand-built ce_ header strings.
    static readonly CloudEventAttribute TraceParent = CloudEventAttribute.CreateExtension("traceparent", CloudEventAttributeType.String);
    static readonly CloudEventAttribute TraceState = CloudEventAttribute.CreateExtension("tracestate", CloudEventAttributeType.String);
    static readonly CloudEventAttribute Redacted = CloudEventAttribute.CreateExtension("redacted", CloudEventAttributeType.Boolean);

    // Structured-body streaming policy: base64 pages hold 3-byte alignment so every non-final segment is
    // padless, and the writer commits past its own buffered measure — the sink grows by page, never by doubling.
    const int SegmentBytes = 48 * 1024;
    const int CommitWatermark = 64 * 1024;

    // StructuredBody streams the webhook leg's application/cloudevents+json body: every populated context attribute renders
    // through its own CloudEventAttribute algebra — booleans and integers as JSON scalars per the structured
    // format, everything else through Format — and data_base64 streams through WriteBase64StringSegment, the
    // final segment closing the string. One envelope vocabulary, two encoders by SIZE posture: the binding's
    // formatter where a transport consumes its own message type whole (Kafka, MQTT), this writer where the body
    // is leg-minted bytes a multi-megabyte payload would otherwise attain as one contiguous base64 string.
    // Exemption: the Utf8JsonWriter body is the platform-forced statement seam the wire-contract law names.
    public static void StructuredBody(CloudEvent envelope, Stream sink) {
        using Utf8JsonWriter writer = new(sink);
        writer.WriteStartObject();
        writer.WriteString("specversion", envelope.SpecVersion.VersionId);
        foreach ((CloudEventAttribute attribute, object value) in envelope.GetPopulatedAttributes()) {
            switch (value) {
                case bool flag: writer.WriteBoolean(attribute.Name, flag); break;
                case int number: writer.WriteNumber(attribute.Name, number); break;
                default: writer.WriteString(attribute.Name, attribute.Format(value)); break;
            }
        }
        if (envelope.Data is ReadOnlyMemory<byte> { IsEmpty: false } data) {
            writer.WritePropertyName("data_base64");
            for (int at = 0; at < data.Length; at += SegmentBytes) {
                int width = Math.Min(SegmentBytes, data.Length - at);
                writer.WriteBase64StringSegment(data.Span.Slice(at, width), isFinalSegment: at + width == data.Length);
                if (writer.BytesPending >= CommitWatermark) { writer.Flush(); }
            }
        }
        writer.WriteEndObject();
    }

    // `Envelope` is the ONE rail: id = content key, source = the oplog URI, type = family.kind, subject =
    // EntityKey (a GraphDelta crossing thereby honors the Element seam envelope vocabulary — the subject is the
    // element's content-address identity, so a broker consumer reads the seam-declared subject without decoding
    // payload bytes), partitionkey = EntityKey RAW (the routing identity — redacting it collapses every
    // partition to one value and destroys per-entity order), sequence = the changefeed sequence. PAYLOAD bytes
    // redact BEFORE the envelope leaves the trust boundary, the `redacted` attribute records that masking
    // fired, and the trace slot continues through the injected `stamp` arrow the AppHost `TraceContext`
    // carrier adapter fills — this projection names the two attribute slots and renders neither value, so
    // every sink leg (webhook, Pulsar, wire-native, AMQP 1.0 included) ships the propagator's own W3C pair
    // and the Kafka/NATS legs mirror those same values onto their native header carriers.
    public static CloudEvent Envelope(
        OpLogEntry row,
        Func<OpLogEntry, (ReadOnlyMemory<byte> Data, bool Redacted)> redact,
        Func<ActivityContext, (string Traceparent, Option<string> Tracestate)> stamp) {
        (ReadOnlyMemory<byte> data, bool masked) = redact(row);
        CloudEvent ce = new() {
            Id = row.ContentKey.ToString("x32"),
            Source = new Uri("rasm:persistence/oplog"),
            Type = $"rasm.oplog.{row.Family.Key}.{row.Kind.Key}",
            Subject = row.EntityKey,
            Time = row.Physical.ToDateTimeOffset(),
            DataContentType = "application/octet-stream",
            Data = data,
        };
        Partitioning.SetPartitionKey(ce, row.EntityKey);
        Sequence.SetSequence(ce, row.Sequence.ToString(System.Globalization.CultureInfo.InvariantCulture));
        if (masked) { ce[Redacted] = true; }
        row.Trace.Continue().Map(stamp).IfSome(pair => {
            ce[TraceParent] = pair.Traceparent;
            pair.Tracestate.IfSome(state => ce[TraceState] = state);
        });
        return ce;
    }
}
```

Selection descriptor — the sentence a row is chosen on, the member a message enters through, the mechanism realizing the closed `none | single | multi` tenancy axis, and who ends a message's life.

| [INDEX] | [SINK]       | [FITS]                          | [ADMIT]                  | [TENANCY]                  | [LIFETIME]                    |
| :-----: | :----------- | :------------------------------ | :----------------------- | :------------------------- | :---------------------------- |
|   [01]  | webhook      | one HTTP consumer, no broker    | `net.http_post` enqueue  | per-tenant target `Uri`    | letter table; no server hold  |
|   [02]  | nats         | low-latency dedup-window fan    | `INatsJSContext.Publish` | account or subject prefix  | `StreamConfig` age/msgs/bytes |
|   [03]  | kafka        | high-volume partition log       | `ProduceAsync`           | topic prefix under ACL     | broker topic retention        |
|   [04]  | rabbitmq     | routed work queue with confirms | `BasicPublishAsync`      | NATIVE vhost               | `Expiration` + queue TTL      |
|   [05]  | amqp         | header-routed `AMQP 1.0` peer   | `SenderLink.SendAsync`   | address prefix             | broker-side, no member        |
|   [06]  | pulsar       | geo-replicated tiered log       | `ISend.Send`             | NATIVE `tenant/namespace`  | namespace retention           |
|   [07]  | wire-native  | in-estate gRPC peer             | `OutboundHop` pipeline   | `TenantContext` on the hop | hop deadline; no persistence  |
|   [08]  | redis-stream | consumer-group work stream      | `StreamAdd`              | key prefix                 | `StreamTrimMode.Acknowledged` |
|   [09]  | clickhouse   | billion-row analytics ingest    | `InsertBinaryAsync`      | tenant-led sort key        | table TTL                     |
|   [10]  | mqtt         | constrained sensor/edge peer    | `IMqttClient.Publish`    | topic prefix               | `WithMessageExpiryInterval`   |

Guarantee coordinates every engine DECIDES for itself, and the differences ARE the point: one value repeating across engines that genuinely differ is a row that stopped reading its engine.

| [INDEX] | [SINK]       | [DELIVER]                          | [ORDER]                      | [SETTLE]                             |
| :-----: | :----------- | :--------------------------------- | :--------------------------- | :----------------------------------- |
|   [01]  | webhook      | at-least-once                      | none                         | reconciled `request_status=SUCCESS`  |
|   [02]  | nats         | at-least-once + dedup window       | subject; NO key member       | `PubAckResponse`, two error surfaces |
|   [03]  | kafka        | at-least-once, idempotent producer | partition by `Message.Key`   | 3-valued `PersistenceStatus`         |
|   [04]  | rabbitmq     | at-least-once, publisher confirms  | queue by routing key; NO key | confirm completion; nack THROWS      |
|   [05]  | amqp         | at-least-once                      | link/address; NO key member  | awaited `SendAsync`; refusal THROWS  |
|   [06]  | pulsar       | at-least-once + fenced leader      | partition + `OrderingKey`    | `MessageId` alone; NO status enum    |
|   [07]  | wire-native  | exactly-once-effective             | none                         | `OutboundHop` honesty verdict        |
|   [08]  | redis-stream | at-least-once                      | stream                       | returned stream id                   |
|   [09]  | clickhouse   | at-least-once                      | none; insert order is no key | awaited insert completion            |
|   [10]  | mqtt         | QoS-1 at-least-once                | topic; NO key member         | PUBACK reason code, never a throw    |

Recovery coordinates — where a re-drive resumes, what bounds in-flight work, and the SHAPE a refusal arrives in, which is the rail trap the `SinkBinding.Watch` cell closes. No row carries a retry schedule: the held cursor owns re-drive for the whole family, and each row's own client or hop owns retry and breaker beneath it.

| [INDEX] | [SINK]       | [REPLAY]                          | [BOUND]                          | [REFUSE]                 |
| :-----: | :----------- | :-------------------------------- | :------------------------------- | :----------------------- |
|   [01]  | webhook      | no origin; receiver `id` key      | pg_net queue + sliding window    | value                    |
|   [02]  | nats         | `DeliverPolicy`, `GetDirectAsync` | `MaxAckPending`, bounded channel | value + throw + event    |
|   [03]  | kafka        | `Seek`/`Offset`/`OffsetsForTimes` | `Flush`/`Poll`/`Pause`           | throw + value + callback |
|   [04]  | rabbitmq     | NONE — head of queue only         | `BasicQos` + confirms limiter    | throw + event            |
|   [05]  | amqp         | NONE; receiver `id` dedup         | this fence's `InFlight` window   | throw + event            |
|   [06]  | pulsar       | `MessageId` seek, cursorless read | pending cap + prefetch           | throw + reactive state   |
|   [07]  | wire-native  | receiver `id` dedup               | hop admission row                | throw (`RpcException`)   |
|   [08]  | redis-stream | `StreamIdempotentId`              | trim by acknowledgement          | throw                    |
|   [09]  | clickhouse   | `insert_deduplication_token`      | pooled insert                    | throw                    |
|   [10]  | mqtt         | session state only; receiver `id` | NONE — the caller bounds it      | value only               |

Honest give-up clause per row: what this transport does NOT do, stated where a reader selecting the row sees it.

| [INDEX] | [SINK]       | [DEGRADE]                                                                      |
| :-----: | :----------- | :----------------------------------------------------------------------------- |
|   [01]  | webhook      | one status per request, and one PENDING holds the cursor for a whole drain     |
|   [02]  | nats         | no key member, so per-entity order needs one subject per entity                |
|   [03]  | kafka        | `Error.IsRetriable` is internal; transactions never span the cursor            |
|   [04]  | rabbitmq     | auto-recovery swallows a drop the caller never observes                        |
|   [05]  | amqp         | no sender credit member at all; this fence bounds its own in-flight            |
|   [06]  | pulsar       | no transactions; a fenced producer surfaces only on `IState`                   |
|   [07]  | wire-native  | no broker behind the hop, so an undelivered envelope rests in the letter table |
|   [08]  | redis-stream | the consumer group's cursor never governs the outbox cursor                    |
|   [09]  | clickhouse   | no events at all; `BeginDbTransaction` throws, the token is the dedup story    |
|   [10]  | mqtt         | no key, no origin, no flow control; v5 fields drop under `V311`                |

## [04]-[RESEARCH]

(none)

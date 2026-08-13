# [PERSISTENCE_VERSION_INGRESS]

`CdcIngress` drains foreign broker topics onto the durable rail — the consume half of the CDC boundary whose egress half `Version/egress` owns. Foreign Kafka records admit through the instrumented consumer twins, decode under the branch owner's DECLARED extension roster, dedup on the `(source, id)` uniqueness composite against the op-log, continue W3C context off message headers, and fold onto the changefeed as first-class ops. Offsets commit only after durable apply, so the broker cursor never outruns the store.

## [01]-[INDEX]

- [02]-[INGRESS_PUMP]: `CdcIngress` folds the instrumented consume leg, the `ResumeOrigin` rebalance resolve under its derived budget, the message envelope decode and source gate, the content-key dedup against the op-log, the durable apply-then-commit law, the `IngressReceipt` conservation and live-edge fold, and the 8500 fault band.

## [02]-[INGRESS_PUMP]

- Owner: `CdcIngress` the static surface owning the one consume fold — instrumented consume, context join, message envelope decode, source gate, atomic content-key apply, offset commit; `IngressSource` the foreign-topic binding row (topic, group, admitted source URI set, batch width, resume origin, poll interval) the composition root fills; `ResumeOrigin` the closed start-position family the rebalance handler dispatches on, its one resolve owned here rather than handed to the composition root as a delegate; `IngressPorts` the injected delegate frame (atomic apply arrow, dead-letter arrow) so no provider type crosses into the fold and no roster arrives from a caller; `IngressReceipt` the per-drain evidence implementing the kernel `IValidityEvidence`; `IngressFault` the 8500 band registered as the `FaultBand.Ingress` row.
- Cases: a consumed record decodes through the branch owner's `EventFormat.Json` formatter under `EventRoster.Declared` into a `CloudEvent`, and four outcomes close the fold beside the one non-record result — `IsPartitionEOF` is a POSITION whose `Message` the client documents as null, so it settles as nothing, stores its offset, stays outside the consumed count, and lands on `AtEdge` as this lane's ONE lag-zero fact — the pump reaches no other idle signal, so suppressing the flag leaves a drain unable to distinguish a caught-up lane from a stalled one — `Applied` (the atomic content-key apply inserted and folded a fresh op, offset stored), `Duplicate` (the same conditional statement observed an existing `(source, id)` pair, skipped and stored — the replay the `Version/egress#EGRESS_SINK` dedup-honesty column promises every producer absorbs), `ForeignSource` (a message envelope whose `source` sits outside the admitted set, dead-lettered), `Refused` (a decode or apply refusal, dead-lettered) — so `consumed == applied + duplicates + deadLettered` is the receipt's conservation law, and `atEdge` tallies beside it as a DISJOINT position count no message envelope ever entered, which is why it rides its own claim rather than widening the conservation sum.
- Entry: `Consume(IConsumer<string, byte[]>, IngressSource, IngressPorts, ProjectionContext, CancellationToken)` admits a positive batch, threads cancellation through the wrapped record settlement, and stores one offset after settlement; `Bind` constructs and subscribes the instrumented consumer, pinning `MaxPollIntervalMs` from the row's `PollInterval` and deriving the rebalance resolve budget from that pinned value, and seats the ONE `SetPartitionsAssignedHandler` this consumer takes — the client raises `InvalidOperationException` on a second set, so the handler binds at exactly one site and no revoked-handler `Func` binds beside it; `Throttle(IConsumer<string, byte[]>, bool)` pauses and resumes FETCH over the held assignment, the one back-pressure verb this client exposes.
- Auto: the rebalance resolve runs on the poll thread INSIDE `Consume`, so every millisecond it spends is charged against `max.poll.interval.ms` and the resolve's whole budget is the `TimeSpan` it passes — an unreachable leader burns that timeout entire and raises `Local_TimedOut`, while the call's own work grows with assignment width at a marginal cost orders beneath the interval, so the timeout DERIVES from the pinned `PollInterval` by the declared `ResolveHeadroom` divisor, and no literal declares that budget a second time; one `OffsetsForTimes` covers the WHOLE held assignment because the client batches its `ListOffsets` per leader, and a chunked resolve is the deleted form on both axes — it multiplies the fixed round trip per chunk and, against a stalled leader, multiplies the timeout by the chunk count, so the guard meant to bound the handler is what overruns the interval; a `Local_TimedOut` folds INSIDE the handler to `Offset.Stored`, the committed-position arm, because an exception leaving the assigned handler propagates out of `Consume` and faults the pump on a rebalance the group otherwise completes; the consumer disables `EnableAutoCommit` and commits through `StoreOffset(consumeResult)` and explicit `Commit` only after each outcome settles durably, so a crash between apply and commit re-consumes and the uniqueness dedup absorbs the replay; dedup identity is the `(source, id)` composite the specification's own uniqueness rule fixes — `id` alone is unique only WITHIN one `source`, so an index on `id` merges two producers' unrelated operations the first time their id spaces overlap — passed into one `TryApply` arrow whose store-owned conditional insert and op fold share one transaction and return `true` only for the winning insert; the message envelope's `subject` crosses `EventKey.Admit`, whose ROUND-TRIP proof refuses the upper-case and short hex spellings a bare `UInt128.TryParse` admits (`"A"` and a full-width key ending `0a` parse to one value and collapse onto one payload identity while each reads correct at its own site); the admitted-source gate reads the message envelope's `source` before apply; the wrapper's processing activity continues the foreign W3C context across the durable apply.
- Receipt: a consume batch rides `store.ingress.consume` carrying the topic, group, consumed/applied/duplicate/dead-lettered counts, the at-edge count, and elapsed duration; a dead-letter rides `store.ingress.deadletter` carrying the message envelope id and the fault; each settled drain receipt fires the `rasm.persistence.ingress.drained` observe point (`Store/observability#HOOK_RAIL`) as a composition-root tap on the drain outcome, the ingress counterpart of the `rasm.persistence.egress.delivered` tap and never an emit call inside the fold.
- Packages: Confluent.Kafka (`ConsumerBuilder<TKey,TValue>`/`ConsumerConfig`/`IConsumer<TKey,TValue>`/`ConsumeResult<TKey,TValue>`/`ConsumeResult.IsPartitionEOF`/`ConsumerConfig.AutoOffsetReset`/`ConsumerConfig.MaxPollIntervalMs`/`ConsumerConfig.EnablePartitionEof`/`ConsumerBuilder.SetPartitionsAssignedHandler`/`OffsetsForTimes`/`TopicPartitionTimestamp`/`TopicPartitionOffset`/`Offset.Stored`/`Timestamp`/`TimestampType.CreateTime`/`KafkaException`/`ErrorCode.Local_TimedOut`/`StoreOffset`/`Commit`/`Subscribe`/`Assignment`/`Pause`/`Resume`), OpenTelemetry.Instrumentation.ConfluentKafka (`AsInstrumentedConsumerBuilder` + `ConfluentKafkaInstrumentedConsumerBuilderOptions` at the bind seam; `ConsumeAndProcessMessageAsync` owns context extraction and the receive/process spans; `AddKafkaConsumerInstrumentation` registers at the AppHost root), CloudNative.CloudEvents (`CloudEvent`, +`.Kafka` `ToCloudEvent`/`IsCloudEvent`), Rasm (`IValidityEvidence`/`ValidityClaim`; `Rasm.Domain` `EventRoster.Declared` the ONE declared extension set, `EventFormat.Json` the one codec identity, `EventKey.Admit` the round-trip content-key gate, `EventCarrier.Read` the propagator's extract accessor), Rasm.Persistence (`Element/graph#FAULT_TABLES` `FaultBand`, `Version/ledger` `OpLogEntry`), LanguageExt.Core, NodaTime, Thinktecture.Runtime.Extensions, BCL inbox.
- Growth: a new foreign topic is one `IngressSource` row at composition — zero fold edits; a new decodable extension is one `EventExtension` row at the branch owner, which both directions read with no edit here; a new admission predicate is one gate row on the source binding; a replay position is one `ResumeOrigin` case; a new ingress transport is one sibling consume leg over the same ports frame, never a second dedup or apply path; zero new surface — an ingress row on the egress binding roster, an auto-committed consumer, a per-topic apply fold, a dedup keyed on broker offset or on `id` alone instead of the uniqueness composite, a caller-supplied extension roster, a raw `ConsumeResult` crossing into the apply arrow, a composition-root resume delegate carrying the resolve law out of this fence, a chunked rebalance resolve, or a bare timeout literal beside the poll interval is the deleted form because the sink family stays egress-only, the offset law is store-first, the message envelope's `id` is the one replay identity, and the resolve budget is derived rather than declared twice.
- Boundary: the producer-to-consumer causal edge is the INSTRUMENTATION's, never this fence's — `ConsumeAndProcessMessageAsync` extracts each record's W3C context and attaches it to that record's process span as an `ActivityLink`, so a hand-composed `TraceCarrier.Link` here mints a second edge for one cause and re-implements shipped capability; the batch commit unit takes no link of its own because a poll-driven fold learns its members only past its own span's creation point and links bind at creation, so batch-to-record causality rides the store-first offset law and the `IngressReceipt` counts rather than a fabricated edge set — the kernel fan-in bracket is for a drain whose members are read BEFORE it opens, which the `Wire/outbox#DISPATCH_SWEEP` relay is and this consume loop is not; the binding roster stays egress-only; consumer construction is Persistence's `Bind` seam while instrumentation registration rides the AppHost root; `TryApply` hands the content key and admitted op to the changefeed owner as one atomic conditional operation, so no read-then-write dedup window exists; `EventRoster.Declared` feeds the typed `ToCloudEvent` overload — a roster arriving as a composition-supplied array is the shape that lets a write-side declaration and a read-side one disagree, and six rostered extensions then decode as untyped strings no typed consumer resolves; binary data folds through the CloudEvents byte glue; the owner's formatter identity pins `AllowDuplicateProperties = false` on BOTH the serializer and the document options, so a duplicate-name attribute in a foreign structured message envelope refuses at decode into the `Refused` arm rather than last-write-winning a hostile value; the processing token reaches every port effect and gates offset storage and batch commit; the rebalance handler is the ONE fence member running outside the wrapped receive/process pair, so it owns its own containment — every resolve refusal folds to a position inside it and nothing propagates, because `Consume` re-raises whatever the handler throws and a rebalance fault surfaces as a consume fault the settle fold never described; `EnablePartitionEof` is pinned TRUE here so the live edge is a result the fold counts, which is the ONE lag-zero fact this lane produces and the reason the settle guard reads `IsPartitionEOF` before any member of a null `Message`.

```csharp signature
// --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
using CloudNative.CloudEvents;
using CloudNative.CloudEvents.Core;
using CloudNative.CloudEvents.Kafka;
using Confluent.Kafka;
using Rasm.Domain;                                // CorrelationId — the S0 causal half the frame seats
using Rasm.Persistence.Element;                   // FaultBand — the one band registry (graph#FAULT_TABLES)
using Rasm.Domain;                                   // Op — the operation key every fallible kernel threads

namespace Rasm.Persistence.Version;

// --- [TYPES] ----------------------------------------------------------------------------

// Closed start-position family the rebalance handler dispatches on. `Committed` answers each held partition with
// `Offset.Stored`, so the group coordinator's own position governs and no second cursor exists to disagree with
// it; `AtTime` resolves a wall-clock instant through ONE broker round trip. Resolution seats in this family
// rather than in a delegate on the binding row because the resolve runs on the poll thread under a derived
// budget, and a composition root handed that delegate owns a timing law it cannot see.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ResumeOrigin {
    private ResumeOrigin() { }

    public sealed record Committed : ResumeOrigin;
    public sealed record AtTime(Instant Wall) : ResumeOrigin;
}

// --- [MODELS] ---------------------------------------------------------------------------

// `IngressSource` binds one foreign topic at composition: broker subscription identity, admitted message
// envelope source set the gate reads, bounded batch width one `Consume` drains, `ResumeOrigin` a rebalance
// resolves against, `Fallback` reset governing a group with NO committed offset at all, and `PollInterval`
// pinned onto the client. `Fallback` rides the row because it decides whether a fresh subscription replays the
// topic's whole retained history or joins live, and leaving it to the client default makes that a
// broker-configuration accident on a rail whose dedup budget is finite; `Error` is the arm a source takes when
// a missing committed offset is itself the fault to surface rather than a position to guess. `PollInterval`
// pins for that same reason AND because the rebalance resolve budget DERIVES from it — an unpinned interval
// leaves that budget reading a client default no row on this page states.
public sealed record IngressSource(
    string Topic,
    string Group,
    FrozenSet<string> Admitted,
    int Batch,
    ResumeOrigin Resume,
    AutoOffsetReset Fallback,
    Duration PollInterval);

// `Uniqueness` carries the composite the specification's own uniqueness rule fixes: `id` is unique only WITHIN
// one `source`, so an index on `id` alone merges two producers' unrelated operations the first time their id
// spaces overlap. Both halves stay producer CLAIMS the admitted-source gate verifies before any routing reads them.
public readonly record struct Uniqueness(string Source, string Id);

// injected delegate frame: `TryApply` atomically claims the uniqueness composite and folds an admitted foreign
// op onto the changefeed, returning true for the winner and false for a duplicate; `DeadLetter` persists the
// refused record — values on a Persistence-owned shape, never a provider type inside the fold. No formatter and
// no extension roster ride here: both are the branch owner's ONE identity, so a composition root cannot hand
// this fold a roster the egress half never wrote under.
public sealed record IngressPorts(
    Func<Uniqueness, UInt128, CloudEvent, ReadOnlyMemory<byte>, CancellationToken, IO<Fin<bool>>> TryApply,
    Func<string, string, CancellationToken, IO<Unit>> DeadLetter);

// Per-drain evidence on the kernel validity floor: every consumed record is applied, deduplicated, or
// dead-lettered, exactly once. `AtEdge` counts end-of-partition POSITIONS and therefore stands OUTSIDE that
// conservation sum — no message envelope was offered, so folding it in inflates a count of records by a count of
// their absence. Reading it: a positive `AtEdge` is this lane's lag-zero evidence, and a zero states only that
// this batch met no edge, never that lag exists — the pump reaches no signal that could claim the negative.
public sealed record IngressReceipt(string Topic, string Group, int Consumed, int Applied, int Duplicates, int DeadLettered, int AtEdge, Duration Elapsed, Instant At, CorrelationId Correlation) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.CountExactly(Applied + Duplicates + DeadLettered, Consumed));
}

// --- [ERRORS] ---------------------------------------------------------------------------
// Band 8500 (`FaultBand.Ingress`): admission faults only — an apply-side store fault stays its own band and
// crosses here only as the dead-letter detail string.
[Union]
public abstract partial record IngressFault : Rasm.Domain.Expected, IValidationError<IngressFault> {
    private IngressFault() : base() { }

    public sealed record EnvelopeRejected(string Detail) : IngressFault;
    public sealed record ForeignSource(string Source) : IngressFault;
    public sealed record ApplyRefused(string EnvelopeId, string Detail) : IngressFault;
    public sealed record CommitRegressed(string Topic, string Detail) : IngressFault;
    public sealed record InvalidBatch(int Found) : IngressFault;

    public override int Code => FaultBand.Ingress + Switch(
        envelopeRejected: static _ => 1,
        foreignSource:    static _ => 2,
        applyRefused:     static _ => 3,
        commitRegressed:  static _ => 4,
        invalidBatch:     static _ => 5);

    public override string Message => Switch(
        envelopeRejected: static c => $"<ingress-envelope:{c.Detail}>",
        foreignSource:    static c => $"<ingress-source:{c.Source}>",
        applyRefused:     static c => $"<ingress-apply:{c.EnvelopeId}>:{c.Detail}",
        commitRegressed:  static c => $"<ingress-commit:{c.Topic}>:{c.Detail}",
        invalidBatch:     static c => $"<ingress-batch:{c.Found}>");

    public override string Category => Switch(
        envelopeRejected: static _ => "Envelope",
        foreignSource:    static _ => "Source",
        applyRefused:     static _ => "Apply",
        commitRegressed:  static _ => "Commit",
        invalidBatch:     static _ => "Batch");

    public static IngressFault Create(string message) => new EnvelopeRejected(message);
}

// --- [OPERATIONS] -----------------------------------------------------------------------

public static class CdcIngress {
    public static readonly StoreSlot ConsumeSlot = StoreSlot.Create("store.ingress.consume");
    public static readonly StoreSlot DeadLetterSlot = StoreSlot.Create("store.ingress.deadletter");
    public static readonly Seq<StoreSlot> Slots = Seq(ConsumeSlot, DeadLetterSlot);

    // Rebalance-resolve headroom: the assigned handler runs on the poll thread INSIDE `Consume`, so whatever it
    // spends is charged against `max.poll.interval.ms`. Dividing the row's pinned interval by this divisor caps
    // that resolve at its own share and leaves the remainder to the settle fold, its one other claimant.
    const int ResolveHeadroom = 4;

    // one construction seam: the plain builder lifts into the instrumented twin with both flags explicit
    // (off-by-default outside DI), auto-commit disabled so the offset law stays store-first.
    public static IConsumer<string, byte[]> Bind(ConsumerConfig config, IngressSource source) {
        config.GroupId = source.Group;
        config.EnableAutoCommit = false;
        config.AutoOffsetReset = source.Fallback;
        // `EnablePartitionEof` ARMS the live-edge result: it is the one idle signal this client offers and the
        // `AtEdge` evidence has no other producer, so suppressing it leaves a caught-up lane and a stalled lane
        // indistinguishable on the receipt. Settle reads `IsPartitionEOF` first because that result documents a
        // null `Message` every later member dereferences.
        config.EnablePartitionEof = true;
        // Pinning the interval makes the resolve budget derivable; an unpinned interval leaves that budget
        // reading a client default no row on this page states.
        config.MaxPollIntervalMs = (int)source.PollInterval.ToTimeSpan().TotalMilliseconds;
        TimeSpan budget = source.PollInterval.ToTimeSpan() / ResolveHeadroom;
        IConsumer<string, byte[]> consumer = new ConsumerBuilder<string, byte[]>(config)
            .AsInstrumentedConsumerBuilder(new ConfluentKafkaInstrumentedConsumerBuilderOptions { EnableTraces = true, EnableMetrics = true })
            // Rebalance is the one moment a start offset is choosable per partition: the handler's
            // `IEnumerable<TopicPartitionOffset>` return REPLACES the committed position, so a source resuming
            // from a wall-clock instant resolves it through `Resolve` here rather than carrying a second cursor
            // no group coordinator ever reads. Setting this handler a second time raises
            // `InvalidOperationException`, so this is its ONE site and no `Func` revoked handler binds beside it.
            .SetPartitionsAssignedHandler((client, held) => Resolve(client, held, source.Resume, budget))
            .Build();
        consumer.Subscribe(source.Topic);
        return consumer;
    }

    // ONE resolve per rebalance over the WHOLE held assignment: this client batches its `ListOffsets` per leader
    // broker, so a chunked call multiplies the fixed round trip AND, against a leader that never answers,
    // multiplies `budget` by the chunk count — turning the guard meant to bound the handler into the thing that
    // overruns the interval. Refusal contains HERE: an unreachable leader burns `budget` whole and raises
    // `Local_TimedOut`, and anything thrown out of an assigned handler re-raises from `Consume`, so a refused
    // resolve answers the committed-position arm and the group completes its rebalance on the coordinator's
    // own cursor rather than faulting a pump whose settle fold never described a rebalance failure.
    static IEnumerable<TopicPartitionOffset> Resolve(IConsumer<string, byte[]> client, List<TopicPartition> held,
        ResumeOrigin origin, TimeSpan budget) =>
        origin.Switch(
            committed: _ => Stored(held),
            atTime:    at => Timed(client, held, at.Wall, budget));

    static List<TopicPartitionOffset> Stored(List<TopicPartition> held) =>
        held.ConvertAll(partition => new TopicPartitionOffset(partition, Offset.Stored));

    static List<TopicPartitionOffset> Timed(IConsumer<string, byte[]> client, List<TopicPartition> held,
        Instant wall, TimeSpan budget) {
        Timestamp at = new(wall.ToDateTimeUtc(), TimestampType.CreateTime);
        try { return client.OffsetsForTimes(held.ConvertAll(partition => new TopicPartitionTimestamp(partition, at)), budget); }
        catch (KafkaException) { return Stored(held); }
    }

    // Back-pressure the client offers no queue-depth member for: `Pause` halts FETCH while the assignment is
    // HELD, so a group whose apply arrow is slower than its topic stops pulling instead of dropping out of the
    // group on a session-timeout the coordinator then rebalances around. `Resume` is the only exit, which is why
    // both verbs seat here over one assignment read rather than inside the settle fold.
    public static Unit Throttle(IConsumer<string, byte[]> consumer, bool halted) {
        List<TopicPartition> held = consumer.Assignment;
        if (halted) { consumer.Pause(held); } else { consumer.Resume(held); }
        return unit;
    }

    // ONE consume fold per record inside the wrapped receive/process span pair: context join -> message
    // envelope decode -> source gate -> content-key dedup -> durable apply -> StoreOffset. Commit lands per batch
    // AFTER every outcome settles, so the broker cursor never outruns the store.
    public static IO<Fin<IngressReceipt>> Consume(IConsumer<string, byte[]> consumer, IngressSource source, IngressPorts ports,
        ProjectionContext frame, CancellationToken token = default) =>
        source.Batch > 0
            ? ConsumeAdmitted(consumer, source, ports, frame, token)
            : IO.pure(Fin<IngressReceipt>.Fail(new IngressFault.InvalidBatch(source.Batch)));

    static IO<Fin<IngressReceipt>> ConsumeAdmitted(IConsumer<string, byte[]> consumer, IngressSource source, IngressPorts ports,
        ProjectionContext frame, CancellationToken token) =>
        from mark in IO.lift(frame.Mark)
        from folded in Range(0, source.Batch).FoldM(
            (Applied: 0, Duplicates: 0, Dead: 0, Edge: 0, Consumed: 0),
            (state, _) => IO.liftAsync(async () => {
                (int applied, int duplicates, int dead, int edge, int consumed) = (0, 0, 0, 0, 0);
                await consumer.ConsumeAndProcessMessageAsync(async (result, _, processingToken) => {
                    // End-of-partition results carry a position the offset law still stores and no message
                    // envelope the conservation fold ever counted, so `Consumed` reads what the broker handed over.
                    consumed = result.IsPartitionEOF || result.Message is null ? 0 : 1;
                    (applied, duplicates, dead, edge) = await Settle(result, source, ports, processingToken).ConfigureAwait(false);
                    processingToken.ThrowIfCancellationRequested();
                    consumer.StoreOffset(result);
                }, token).ConfigureAwait(false);
                return (state.Applied + applied, state.Duplicates + duplicates, state.Dead + dead, state.Edge + edge, state.Consumed + consumed);
            })).As()
        from committed in IO.lift(() => {
            token.ThrowIfCancellationRequested();
            try { consumer.Commit(); return Fin<Unit>.Succ(unit); }
            catch (Exception failure) { return Fin<Unit>.Fail(new IngressFault.CommitRegressed(source.Topic, failure.Message)); }
        })
        select committed.Map(_ => new IngressReceipt(
            source.Topic, source.Group, folded.Consumed, folded.Applied, folded.Duplicates, folded.Dead, folded.Edge,
            frame.Elapsed(mark), frame.Now(), frame.Correlation));

    // `IsPartitionEOF` is a POSITION, not a record: the client documents `Message` as null on that result, so
    // every member read past this guard — the `IsCloudEvent` probe included — dereferences null on the first
    // consumer that reaches the end of a partition. An EOF result counts on `Edge` alone rather than as a dead
    // letter, because no message envelope was offered and the conservation fold counts what the broker handed —
    // reaching the live edge is the lane's lag-zero fact, and grading it a refusal would dead-letter an absence.
    static async ValueTask<(int Applied, int Duplicates, int Dead, int Edge)> Settle(ConsumeResult<string, byte[]> result,
        IngressSource source, IngressPorts ports, CancellationToken token) {
        token.ThrowIfCancellationRequested();
        if (result.IsPartitionEOF) return (0, 0, 0, 1);
        if (result.Message is null) return (0, 0, 0, 0);
        if (!result.Message.IsCloudEvent()) return await Dead(ports, "<not-cloudevent>", result.Topic, token).ConfigureAwait(false);

        // `EventRoster.Declared` crosses at the read exactly as it crossed at the mint, so a rostered extension
        // decodes typed rather than as the untyped string a caller-supplied array leaves behind.
        CloudEvent envelope;
        try { envelope = result.Message.ToCloudEvent(EventFormat.Json.Formatter, EventRoster.Declared); }
        catch (OperationCanceledException) when (token.IsCancellationRequested) { throw; }
        catch (Exception failure) {
            return await Dead(ports, new IngressFault.EnvelopeRejected(failure.Message).Message, result.Topic, token).ConfigureAwait(false);
        }

        if (envelope.Source is not { } origin || !source.Admitted.Contains(origin.ToString()))
            return await Dead(ports, new IngressFault.ForeignSource(envelope.Source?.ToString() ?? "<none>").Message,
                envelope.Id ?? "<none>", token).ConfigureAwait(false);

        if (envelope.Id is not { Length: > 0 } envelopeId)
            return await Dead(ports, new IngressFault.EnvelopeRejected("<absent-operation-id>").Message, "<none>", token).ConfigureAwait(false);

        // `EventKey.Admit` proves the round trip over the content key riding `subject`, never the parse alone: a
        // bare `UInt128.TryParse` under `HexNumber` admits upper-case digits and short forms this fabric never
        // emits, so `"A"` and a full-width key ending `0a` collapse onto one payload identity while each reads
        // correct at its own site. `id` stays the OPERATION identity and never enters this conversion.
        if (EventKey.Admit(envelope.Subject, Op.Of()) is not { IsSucc: true } admitted)
            return await Dead(ports, new IngressFault.EnvelopeRejected("<invalid-content-key>").Message, envelopeId, token).ConfigureAwait(false);
        UInt128 contentKey = admitted.ValueUnsafe();

        ReadOnlyMemory<byte> payload;
        try {
            token.ThrowIfCancellationRequested();
            payload = envelope.Data switch {
                byte[] bytes => bytes,
                ReadOnlyMemory<byte> memory => memory,
                Memory<byte> memory => memory,
                Stream stream => BinaryDataUtilities.ToReadOnlyMemory(stream),
                _ => throw new InvalidDataException("<unsupported-cloudevent-data>"),
            };
        } catch (OperationCanceledException) when (token.IsCancellationRequested) { throw; }
        catch (Exception failure) {
            return await Dead(ports, new IngressFault.EnvelopeRejected(failure.Message).Message, envelopeId, token).ConfigureAwait(false);
        }

        Fin<bool> outcome = await ports.TryApply(new Uniqueness(origin.ToString(), envelopeId), contentKey, envelope, payload, token)
            .RunAsync(token).ConfigureAwait(false);
        return outcome.IsFail
            ? await Dead(ports, new IngressFault.ApplyRefused(envelopeId, outcome.Error.Message).Message, envelopeId, token).ConfigureAwait(false)
            : outcome.ValueUnsafe() ? (1, 0, 0, 0) : (0, 1, 0, 0);
    }

    static async ValueTask<(int Applied, int Duplicates, int Dead, int Edge)> Dead(IngressPorts ports, string fault, string key,
        CancellationToken token) {
        await ports.DeadLetter(fault, key, token).RunAsync(token).ConfigureAwait(false);
        return (0, 0, 1, 0);
    }
}
```

| [INDEX] | [POLICY]        | [VALUE]                                          | [BINDING]                                                 |
| :-----: | :-------------- | :----------------------------------------------- | :-------------------------------------------------------- |
|   [01]  | consume seam    | instrumented builder twin, flags explicit        | `AddKafkaConsumerInstrumentation` rides the AppHost root  |
|   [02]  | context join    | instrumented wrapper extraction                  | one receive/process span pair wraps every record          |
|   [03]  | dedup identity  | the `(source, id)` composite vs the op-log index | `id` alone merges two producers' unrelated operations     |
|   [04]  | offset law      | `StoreOffset` after settle, batch `Commit`       | auto-commit disabled; the cursor never outruns the store  |
|   [05]  | source gate     | admitted message envelope source set             | a mixed topic cannot leak an unvetted op onto the rail    |
|   [06]  | family split    | ingress owner beside the egress sink family      | the binding roster stays egress-only; no shared row       |
|   [07]  | resume origin   | the row's `ResumeOrigin` case                    | a fresh group's start position is a row, not a default    |
|   [08]  | resolve shape   | ONE `OffsetsForTimes` over the whole assignment  | chunking multiplies the round trip AND the timeout        |
|   [09]  | resolve budget  | `PollInterval` over `ResolveHeadroom`            | the poll thread's own share, never a bare literal         |
|   [10]  | resolve refusal | `Local_TimedOut` folds to `Offset.Stored`        | a handler throw re-raises out of `Consume`                |
|   [11]  | edge evidence   | `EnablePartitionEof` armed, counted on `AtEdge`  | positive reads lag-zero; zero claims nothing              |
|   [12]  | bound           | `Pause`/`Resume` over the held assignment        | fetch halts; the assignment survives the session timeout  |
|   [13]  | refusal shape   | `ConsumeException` throws, `Error` callbacks     | the settle fold reads both; neither rail sees the other   |
|   [14]  | decode roster   | the owner's `EventRoster.Declared`, both ways    | a caller-supplied array decodes rostered rows untyped     |
|   [15]  | content key     | `EventKey.Admit` round trip over `subject`       | a bare parse collapses upper-case and short hex spellings |

## [03]-[RESEARCH]

(none)

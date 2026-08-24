# [PERSISTENCE_VERSION_INGRESS]

`CdcIngress` drains foreign broker topics onto the durable rail — the consume half of the CDC boundary whose egress half `Version/egress` owns. Foreign Kafka records admit through the instrumented consumer twins, decode through the generated extension contract, dedup on the `(source, id)` uniqueness composite against the op-log, continue W3C context off message headers, and fold onto the changefeed as first-class ops. Offsets commit only after durable apply, so the broker cursor never outruns the store.

## [01]-[INDEX]

- [02]-[INGRESS_PUMP]: `CdcIngress` folds the instrumented consume leg, the `ResumeOrigin` rebalance resolve under its derived budget, the message envelope decode and source gate, the content-key dedup against the op-log, the durable apply-then-commit law, the `IngressReceipt` conservation and live-edge fold, and the 8500 fault band.

## [02]-[INGRESS_PUMP]

- Owner: `CdcIngress` the static surface owning the one consume fold — instrumented consume, context join, message envelope decode, generated-profile admission, source gate, content acquisition, atomic apply, offset commit; `IngressSource` the foreign-topic binding row (topic, group, admitted source URI set, batch width, resume origin, poll interval) the composition root fills; `ResumeOrigin` the closed start-position family the rebalance handler dispatches on, its one resolve owned here rather than handed to the composition root as a delegate; `IngressPayload` the inline-or-reference carrier; `IngressPorts` the injected resolve/apply/dead-letter frame so no provider type crosses into the fold and no roster arrives from a caller; `AdmittedRecord` the proved foreign record the apply leg takes, so nothing past admission re-validates; `IngressOutcome` the closed per-record settlement whose rows carry their own `IngressTally` step, making the conservation law structural; `IngressReceipt` the per-drain evidence implementing the kernel `IValidityEvidence`; `[FaultCase]`/`IngressFault` the 8500 band on the kernel `[FaultCase]`/`Fault` floor, one roster row per concern with `Code` sealed.
- Cases: a consumed record decodes through `EventFormat.Json` with declarations from `EventExtensionContract<Extensions>`, which reconstructs and validates the whole generated message. Inline bytes and a `dataref` URI-reference are the two payload arms; four message outcomes plus the partition-edge position close the conservation fold.
- Entry: `Consume(IConsumer<string, byte[]>, IngressSource, IngressPorts, ProjectionContext, CancellationToken)` admits a positive batch, threads cancellation through the wrapped record settlement, and stores one offset after settlement; `Bind` constructs and subscribes the instrumented consumer, pinning `MaxPollIntervalMs` from the row's `PollInterval` and deriving the rebalance resolve budget from that pinned value, and seats the ONE `SetPartitionsAssignedHandler` this consumer takes — the client raises `InvalidOperationException` on a second set, so the handler binds at exactly one site and no revoked-handler `Func` binds beside it; `Throttle(IConsumer<string, byte[]>, bool)` pauses and resumes FETCH over the held assignment, the one back-pressure verb this client exposes; `Close(IConsumer<string, byte[]>, IngressSource, Op)` is the ONE teardown, committing stored offsets and leaving the group where a bare `Dispose` does neither.
- Auto: the rebalance resolve runs on the poll thread INSIDE `Consume`, so every millisecond it spends is charged against `max.poll.interval.ms` and the resolve's whole budget is the `TimeSpan` it passes — an unreachable leader burns that timeout entire and raises `Local_TimedOut`, while the call's own work grows with assignment width at a marginal cost orders beneath the interval, so the timeout DERIVES from the pinned `PollInterval` by the declared `ResolveHeadroom` divisor, and no literal declares that budget a second time; one `OffsetsForTimes` covers the WHOLE held assignment because the client batches its `ListOffsets` per leader, and a chunked resolve is the deleted form on both axes — it multiplies the fixed round trip per chunk and, against a stalled leader, multiplies the timeout by the chunk count, so the guard meant to bound the handler is what overruns the interval; a refused resolve folds INSIDE the handler to `Offset.Stored`, the committed-position arm, because an exception leaving the assigned handler propagates out of `Consume` and faults the pump on a rebalance the group otherwise completes — and the containment is the kernel `Op.Catch` funnel over the WHOLE crossing rather than a `Local_TimedOut` filter, since a narrower provider-type catch left a disposed client or a cancelled budget wait free to escape the one member that must not throw; the consumer disables `EnableAutoCommit` and commits through `StoreOffset(consumeResult)` and explicit `Commit` only after each outcome settles durably, so a crash between apply and commit re-consumes and the uniqueness dedup absorbs the replay; dedup identity is the `(source, id)` composite the specification's own uniqueness rule fixes — `id` alone is unique only WITHIN one `source`, so an index on `id` merges two producers' unrelated operations the first time their id spaces overlap — passed into one `TryApply` arrow whose store-owned conditional insert and op fold share one transaction and return `true` only for the winning insert; the message envelope's `subject` crosses `ContentHash.Admit`, whose ROUND-TRIP proof refuses the upper-case and short hex spellings a bare `UInt128.TryParse` admits (`"A"` and a full-width key ending `0a` parse to one value and collapse onto one payload identity while each reads correct at its own site); the admitted-source gate reads the message envelope's `source` before apply; the wrapper's processing activity continues the foreign W3C context across the durable apply; that same wrapper returns a null-message or partition-edge result WITHOUT invoking its handler, so the settlement fold reads BOTH halves of it — the callback for record-bearing results and the return value for the two positions that carry no record — and a fold reading only the callback arms `EnablePartitionEof` for an edge count structurally pinned at zero; the wrapper also refuses any consumer that is not the instrumented twin, raising `ArgumentException` on a plain `IConsumer`, so `Bind` is the one construction path and a caller substituting a bare client fails at the first drain rather than at build; teardown crosses `Close` because a bare `Dispose` neither commits stored offsets nor tells the coordinator the member left, stranding the whole assignment for `session.timeout.ms`.
- Receipt: a consume batch rides `store.ingress.consume` carrying the topic, group, consumed/applied/duplicate/dead-lettered counts, the at-edge count, and elapsed duration; a dead-letter rides `store.ingress.deadletter` carrying the fault-derived routing key and generated `FaultObservation` projected by AppHost composition; each settled drain receipt fires the `rasm.persistence.ingress.drained` observe point (`Store/observability#HOOK_RAIL`) as a composition-root tap on the drain outcome, the ingress counterpart of the `rasm.persistence.egress.delivered` tap and never an emit call inside the fold.
- Packages: Confluent.Kafka, OpenTelemetry.Instrumentation.ConfluentKafka, CloudNative.CloudEvents.Kafka, Rasm.Contracts, Celly.Protovalidate, Rasm, LanguageExt.Core, NodaTime, Thinktecture.Runtime.Extensions, BCL inbox.
- Growth: a new foreign topic is one `IngressSource` row; a consumed extension changes `event.proto`; a new settlement is one `IngressOutcome` row.
- Boundary: Kafka instrumentation owns causal links; Persistence owns consumer construction and contributes no extension roster. A reference-only event resolves through the injected residence port before apply, and both payload arms re-hash against `subject` before the store sees bytes. `TryApply` remains atomic, duplicate JSON keys refuse, processing cancellation reaches every effect, and the rebalance handler contains its own resolution faults.

```csharp signature
// --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
using CloudNative.CloudEvents;
using CloudNative.CloudEvents.Core;
using CloudNative.CloudEvents.Kafka;
using Confluent.Kafka;
using Error = LanguageExt.Common.Error;
using System.Text.Json;
// CorrelationId the S0 causal half the frame seats, Op the key every fallible kernel threads, Fault the kernel
// substrate union `Op.Catch` keys a cancellation into, and FaultBand/[FaultCase]/Fault the fault-estate
// floor this band rides — all `Rasm.Domain`. `Element/graph#FAULT_TABLES` ROUTES at that roster and declares no
// rows, so it names no namespace of its own and nothing here imports one for the band.
using Rasm.Domain;

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

// Ingress's closed outcome vocabulary carries the tally step each row makes, so the conservation law is STRUCTURAL
// rather than a claim re-derived at each site. The four-slot `(applied, duplicates, dead, edge)` int
// tuple this replaces forged three zeros at every return and left `Consumed` to a separate hand-branch over
// `IsPartitionEOF || Message is null`, which is exactly the discrimination the rows already make. `AtEdge` steps
// `Edge` and NOT `Consumed`: no message envelope was offered, so folding it in counts records by their absence.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class IngressOutcome {
    public static readonly IngressOutcome Applied      = new(key: "applied",      count: static t => t with { Consumed = t.Consumed + 1, Applied = t.Applied + 1 });
    public static readonly IngressOutcome Duplicate    = new(key: "duplicate",    count: static t => t with { Consumed = t.Consumed + 1, Duplicates = t.Duplicates + 1 });
    public static readonly IngressOutcome DeadLettered = new(key: "deadlettered", count: static t => t with { Consumed = t.Consumed + 1, Dead = t.Dead + 1 });
    public static readonly IngressOutcome AtEdge       = new(key: "atedge",       count: static t => t with { Edge = t.Edge + 1 });
    public static readonly IngressOutcome Absent       = new(key: "absent",       count: static t => t);
    private readonly Func<IngressTally, IngressTally> count;
    public IngressTally Count(IngressTally tally) => count(tally);
}

// Payload placement is a closed wire fact. `Inline` preserves a body the binding carried; `External` preserves the
// URI-reference the generated `dataref` field admitted. The apply boundary acquires either arm and verifies the
// resulting bytes against `subject`, so a resolver cannot substitute content under a valid-looking address.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record IngressPayload {
    private IngressPayload() { }
    public sealed record Inline(ReadOnlyMemory<byte> Bytes) : IngressPayload;
    public sealed record External(Uri Reference) : IngressPayload;
}

// Drain state accumulates here, owned beside the rows that step it.
public readonly record struct IngressTally(int Consumed, int Applied, int Duplicates, int Dead, int Edge) {
    public static readonly IngressTally Zero = default;
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
    Dimension Batch,
    ResumeOrigin Resume,
    AutoOffsetReset Fallback,
    Duration PollInterval);

// `Uniqueness` carries the composite the specification's own uniqueness rule fixes: `id` is unique only WITHIN
// one `source`, so an index on `id` alone merges two producers' unrelated operations the first time their id
// spaces overlap. Both halves stay producer CLAIMS the admitted-source gate verifies before any routing reads them.
public readonly record struct Uniqueness(string Source, string Id);

// One admitted foreign record: the uniqueness composite, the content key that rode `subject`, the decoded envelope,
// the admitted generated profile, and the closed payload placement. Every column is PROVED before construction, so
// the apply leg re-validates nothing and no raw `ConsumeResult` or unvetted attribute reaches past this shape.
public readonly record struct AdmittedRecord(
    Uniqueness Key,
    UInt128 ContentKey,
    CloudEvent Envelope,
    global::Rasm.Contracts.Event.Extensions Extensions,
    IngressPayload Payload);

// injected delegate frame: `TryApply` atomically claims the uniqueness composite and folds an admitted foreign
// op onto the changefeed, returning true for the winner and false for a duplicate; `DeadLetter` persists the
// refused record — values on a Persistence-owned shape, never a provider type inside the fold. No formatter and
// no extension roster ride here: both are the branch owner's ONE identity, so a composition root cannot hand
// this fold a roster the egress half never wrote under. `ObserveFault` is the injected AppHost `FaultWire.Observe`
// projection; `DeadLetter` stores the generated observation without giving Persistence an AppHost dependency or a
// second fault grammar.
public sealed record IngressPorts(
    Func<Uri, CancellationToken, IO<Fin<ReadOnlyMemory<byte>>>> Resolve,
    Func<Uniqueness, UInt128, CloudEvent, global::Rasm.Contracts.Event.Extensions, ReadOnlyMemory<byte>, CancellationToken, IO<Fin<bool>>> TryApply,
    Func<Error, global::Rasm.Contracts.Fault.FaultObservation> ObserveFault,
    Func<global::Rasm.Contracts.Fault.FaultObservation, string, CancellationToken, IO<Unit>> DeadLetter);

// Per-drain evidence on the kernel validity floor: every consumed record is applied, deduplicated, or
// dead-lettered, exactly once. `AtEdge` counts end-of-partition POSITIONS and therefore stands OUTSIDE that
// conservation sum — no message envelope was offered, so folding it in inflates a count of records by a count of
// their absence. Reading it: a positive `AtEdge` is this lane's lag-zero evidence, and a zero states only that
// this batch met no edge, never that lag exists — the pump reaches no signal that could claim the negative.
public sealed record IngressReceipt(string Topic, string Group, IngressTally Tally, Duration Elapsed, Instant At, CorrelationId Correlation) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.CountExactly(Tally.Applied + Tally.Duplicates + Tally.Dead, Tally.Consumed));
}

// --- [ERRORS] ---------------------------------------------------------------------------
// Band 8500 (`FaultBand.Ingress`): admission faults only — an apply-side store fault stays its own band and
// passes through unchanged. `Fault`-derived on the kernel floor, so `Code` and
// Generated identity derives from `[FaultCase]`; this family spells no manual band or secondary classification read of its
// own; the case lifts BARE onto `Fin<T>` and a recovery reads `error.IsType<IngressFault.ForeignSource>()`.
// `Envelope` is OPTION-shaped on the two arms that can refuse before a record names an id — the `?? "<none>"`
// sentinel that stood there made every consumer parse a magic string back into the absence it already was.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record IngressFault : Fault {
    private static readonly FaultBand FamilyBand = FaultBand.Ingress;
    private IngressFault() { }
    [FaultCase(0)]
    public sealed partial record InvalidEnvelope(Option<string> Envelope, string Detail) : IngressFault();

    [FaultCase(1)]
    public sealed partial record EnvelopeRejected(Option<string> Envelope, Error Cause) : IngressFault(), ICausedFault;
    [FaultCase(2)]
    public sealed partial record ForeignSource(Option<string> Envelope, Option<string> Source) : IngressFault();
    [FaultCase(3)]
    public sealed partial record ApplyRefused(string Envelope, Error Cause) : IngressFault(), ICausedFault;
    [FaultCase(4)]
    public sealed partial record CommitRegressed(string Topic, Error Cause) : IngressFault(), ICausedFault;
    // Teardown carries its own case because a `Close` re-raising a stashed handler exception is not a commit
    // refusal, and folding it onto `CommitRegressed` would report an offset regression on a drain whose offsets
    // committed cleanly.
    [FaultCase(5)]
    public sealed partial record CloseAbandoned(string Group, Error Cause) : IngressFault(), ICausedFault;
    public override string Message => Switch(
        invalidEnvelope:  static c => $"<ingress-envelope:{c.Detail}>",
        envelopeRejected: static c => $"<ingress-envelope:{c.Cause.Message}>",
        foreignSource:    static c => $"<ingress-source:{c.Source.IfNone("<absent>")}>",
        applyRefused:     static c => $"<ingress-apply:{c.Envelope}>:{c.Cause.Message}",
        commitRegressed:  static c => $"<ingress-commit:{c.Topic}>:{c.Cause.Message}",
        closeAbandoned:   static c => $"<ingress-close:{c.Group}>:{c.Cause.Message}");

    // `Route` derives the dead-letter ROUTING key: the message envelope id where a record named one, and the
    // refusing topic where refusal preceded any id. ONE projection on the union, so no call site carries a
    // hand-spelled key beside the fault that already knows it.
    public string Route(string topic) => Switch(
        invalidEnvelope:  c => c.Envelope.IfNone(topic),
        envelopeRejected: c => c.Envelope.IfNone(topic),
        foreignSource:    c => c.Envelope.IfNone(topic),
        applyRefused:     c => c.Envelope,
        commitRegressed:  c => c.Topic,
        closeAbandoned:   c => c.Group);

    // `Lift` is the ONE inbound throw conversion on this rail — every throw crossing rides the kernel
    // `Op.Catch` funnel; only a documented exception family lands here, so no leg spells its own `catch`. A cancellation arrives already keyed
    // as `KernelFault.Cancelled` and passes through UNBANDED, which is what lets the settle fold tell a cancelled
    // drain from a malformed record: dead-lettering the first writes a refusal for a record nothing judged and
    // stores an offset past a record nothing applied. An already-banded fault passes through, so a nested
    // crossing never re-wraps. `arm` is the refusing STAGE's own case — the error carries no clue which — and
    // `recognizes` proves the provider contract before minting it; every unknown or already-typed error stays exact.
    public static Error Lift(Error error, Func<Exception, bool> recognizes, Func<Error, IngressFault> arm) => error switch {
        Fault => error,
        { Exception.Case: Exception raised } when recognizes(raised) => arm(error),
        _ => error,
    };
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
            .SetPartitionsAssignedHandler((client, held) => Resolve(client, held, source.Resume, budget, Op.Of()))
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
        ResumeOrigin origin, TimeSpan budget, Op key) =>
        origin.Switch(
            committed: _ => Stored(held),
            atTime:    at => Timed(client, held, at.Wall, budget, key));

    static List<TopicPartitionOffset> Stored(List<TopicPartition> held) =>
        held.ConvertAll(partition => new TopicPartitionOffset(partition, Offset.Stored));

    // `Op.Catch` folds the entire assigned-handler crossing, and the committed-position arm answers the result.
    static List<TopicPartitionOffset> Timed(IConsumer<string, byte[]> client, List<TopicPartition> held,
        Instant wall, TimeSpan budget, Op key) {
        Timestamp at = new(wall.ToDateTimeUtc(), TimestampType.CreateTime);
        return key.Catch(() => Fin.Succ(client.OffsetsForTimes(
                held.ConvertAll(partition => new TopicPartitionTimestamp(partition, at)), budget)))
            .IfFail(_ => Stored(held));
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

    // Teardown COMMITS, never cancels. `Dispose` alone releases the handle "without committing offsets and
    // without alerting the group coordinator that the consumer is exiting the group", stranding the whole
    // assignment until `session.timeout.ms` expires — so every offset this drain stored past its last batch
    // `Commit` falls to a re-consume the dedup then absorbs, while the partitions sit unserved for that timeout
    // and the coordinator waits on a member that already left. `Close` commits the stored state and leaves the
    // group in one call, so it owns teardown here rather than at a caller reaching for `Dispose`. `Op.Catch`
    // wraps it because `Close` re-raises a statistics- or log-handler exception stashed from an earlier callback
    // — a throw unrelated to the close, arriving on the one path that must not escape — and because a rethrown
    // handler fault must not read as a commit refusal, it lifts to its own case rather than `CommitRegressed`.
    public static IO<Fin<Unit>> Close(IConsumer<string, byte[]> consumer, IngressSource source, Op key) =>
        IO.lift(() => key.Catch(() => { consumer.Close(); return Fin.Succ(unit); })
            .MapFail(error => IngressFault.Lift(error,
                static raised => raised is KafkaException or ObjectDisposedException,
                cause => new IngressFault.CloseAbandoned(source.Group, cause))));

    // ONE consume fold per record inside the wrapped receive/process span pair: context join -> message
    // envelope decode -> source gate -> content-key dedup -> durable apply -> StoreOffset. Commit lands per batch
    // AFTER every outcome settles, so the broker cursor never outruns the store.
    public static IO<Fin<IngressReceipt>> Consume(IConsumer<string, byte[]> consumer, IngressSource source, IngressPorts ports,
        ProjectionContext frame, CancellationToken token = default) =>
        ConsumeAdmitted(consumer, source, ports, frame, Op.Of(), token);

    static IO<Fin<IngressReceipt>> ConsumeAdmitted(IConsumer<string, byte[]> consumer, IngressSource source, IngressPorts ports,
        ProjectionContext frame, Op key, CancellationToken token) =>
        from mark in IO.lift(frame.Mark)
        from folded in Range(0, source.Batch.Value).FoldM(IngressTally.Zero, (tally, _) => IO.liftAsync(async () =>
            (await key.Catch(async _ => {
            Option<Error> refused = None;
            Option<IngressOutcome> settled = None;
            // Wrapper RETURN produces the non-record outcomes and its callback the record ones, so both are read.
            ConsumeResult<string, byte[]>? offered = await consumer.ConsumeAndProcessMessageAsync(async (result, _, processingToken) => {
                // A failed settlement is retained as the exact rail value and suppresses offset storage. The
                // callback returns normally because throwing an ErrorException here would make the outer capture
                // reclassify it as a fresh exceptional Error and lose the typed fault identity it already carries.
                // `StoreOffset` takes the WHOLE result and raises `InvalidOperationException` on an empty one, which
                // is safe here for the same reason `Settle` takes a bare message: the wrapper already proved this
                // result carries one.
                (await Settle(result.Message, source, ports, key, processingToken).ConfigureAwait(false)).Match(
                    Succ: outcome => {
                        processingToken.ThrowIfCancellationRequested();
                        settled = Some(outcome);
                        consumer.StoreOffset(result);
                        return unit;
                    },
                    Fail: error => {
                        refused = Some(error);
                        return unit;
                    });
            }, token).ConfigureAwait(false);
            return refused.Match(
                Some: Fin<IngressTally>.Fail,
                None: () => Fin.Succ(settled.IfNone(() => Positioned(offered)).Count(tally)));
        }).ConfigureAwait(false)).MapFail(error => IngressFault.Lift(error,
            static raised => raised is KafkaException,
            cause => new IngressFault.EnvelopeRejected(None, cause))))).Bind(IO.liftFin)).As()
        from committed in IO.lift(() => {
            token.ThrowIfCancellationRequested();
            return key.Catch(() => { consumer.Commit(); return Fin.Succ(unit); })
                .MapFail(error => IngressFault.Lift(error,
                    static raised => raised is KafkaException,
                    cause => new IngressFault.CommitRegressed(source.Topic, cause)));
        })
        select committed.Map(_ => new IngressReceipt(
            source.Topic, source.Group, folded, frame.Elapsed(mark), frame.Now(), frame.Correlation));

    // `IsPartitionEOF` is a POSITION, not a record: the client documents `Message` as null on that result, so every
    // member read past it — the `IsCloudEvent` probe included — dereferences null at the first live edge. The
    // instrumented wrapper enforces that itself, answering `if (consumeResult?.Message == null ||
    // consumeResult.IsPartitionEOF) return consumeResult;` BEFORE it opens the process span, so the callback is
    // handed record-bearing results ALONE and the two non-record outcomes have exactly ONE producer: the wrapper's
    // return value. Reading them off the callback instead leaves `AtEdge` and `Absent` structurally unreachable —
    // `EnablePartitionEof` stays armed, the receipt publishes a permanent zero for the one idle signal this client
    // offers, and a caught-up lane reads identically to a stalled one while the policy row claims otherwise.
    static IngressOutcome Positioned(ConsumeResult<string, byte[]>? offered) =>
        offered is { IsPartitionEOF: true } ? IngressOutcome.AtEdge : IngressOutcome.Absent;

    // Settlement takes the PROVED message rather than the result, because the wrapper already discharged absence:
    // an arm here re-testing `IsPartitionEOF` or a null `Message` would be a second guard over a boundary that
    // cannot deliver either, and a dead arm reads as a handled case no probe ever reaches.
    static async ValueTask<Fin<IngressOutcome>> Settle(Message<string, byte[]> message, IngressSource source,
        IngressPorts ports, Op key, CancellationToken token) =>
        await Admit(message, source, key).Match(
            Succ: admitted => Applied(admitted, ports, source.Topic, key, token),
            Fail: fault => Dead(ports, fault, source.Topic, token)).ConfigureAwait(false);

    // ONE admission per record, admitted ONCE at this boundary so the apply leg re-validates nothing: the
    // CloudEvents probe and roster decode, the source gate, the operation id, the content key, and the payload
    // shape fold into dependent `Fin` clauses whose refusals are already the banded cases their sites name. The chain
    // SEQUENCES rather than accumulates because each step consumes the value the one before it produced — an
    // envelope has no `source` until it decodes — so an applicative fold here would report faults derived from
    // values that were never produced. Separate `Dead(…)` returns here made writing the dead
    // letter a per-step obligation any new step could silently forget; refusal now has exactly one exit.
    static Fin<AdmittedRecord> Admit(Message<string, byte[]> message, IngressSource source, Op key) =>
        from envelope in Decoded(message, key)
        from extensions in EgressEventExtensions.Contract.Admit(envelope, key)
            .MapFail(error => new IngressFault.EnvelopeRejected(Optional(envelope.Id), error))
        from origin in Admitted(envelope, source)
        from id in Identified(envelope, key)
// `ContentHash.Admit` proves the ROUND TRIP over the content key riding `subject`, never the parse alone: a
        // bare `UInt128.TryParse` under `HexNumber` admits upper-case digits and short forms this fabric never
        // emits, so `"A"` and a full-width key ending `0a` collapse onto one payload identity while each reads
        // correct at its own site. `id` stays the OPERATION identity and never enters this conversion.
        from subject in Optional(envelope.Subject)
            .ToFin(new IngressFault.InvalidEnvelope(Optional(envelope.Id), "<absent-content-key>"))
        from content in ContentHash.Admit(hex: subject, key: key)
            .MapFail(error => new IngressFault.EnvelopeRejected(Some(id.ToString()), error))
        from payload in Payload(envelope, extensions, id.ToString(), key)
        select new AdmittedRecord(new Uniqueness(origin, id.ToString()), content, envelope, extensions, payload);

    // The producer's generated-descriptor declaration crosses at the read exactly as it crossed at mint. The
    // formatter decodes typed rather than as the untyped string a caller-supplied array leaves behind; the owner's
    // formatter identity pins `AllowDuplicateProperties = false`, so a duplicate-name attribute in a hostile
    // structured message envelope refuses HERE rather than last-write-winning a value nothing vetted.
    static Fin<CloudEvent> Decoded(Message<string, byte[]> message, Op key) =>
        message.IsCloudEvent()
            ? from declared in EgressEventExtensions.Contract.Declarations(key)
              from envelope in key.Catch(() => Fin.Succ(message.ToCloudEvent(EventFormat.Json.Formatter, declared)))
                  .Bind(admitted => EventEnvelope.Admit(admitted, key))
                  .MapFail(error => IngressFault.Lift(error,
                      static raised => raised is ArgumentException or JsonException,
                      cause => new IngressFault.EnvelopeRejected(None, cause)))
              select envelope
            : Fin<CloudEvent>.Fail(new IngressFault.InvalidEnvelope(None, "<not-cloudevent>"));

    // Admission reads the message envelope's `source` BEFORE any apply, so a mixed topic cannot
    // leak an unvetted op onto the rail. An ABSENT source and an unadmitted one are one refusal — neither names
    // an admitted producer — and the fault carries the claim as an `Option` rather than a sentinel standing where
    // a producer's name would be.
    static Fin<string> Admitted(CloudEvent envelope, IngressSource source) {
        Option<string> named = Optional(envelope.Source).Map(static origin => origin.ToString());
        return named.Filter(source.Admitted.Contains)
            .ToFin(new IngressFault.ForeignSource(Optional(envelope.Id), named));
    }

    // `id` is the OPERATION identity the uniqueness composite carries; an absent or empty one leaves that pair
    // unformable, so it refuses before any store round trip rather than deduping against a blank.
    static Fin<EventId> Identified(CloudEvent envelope, Op key) =>
        Optional(envelope.Id)
            .ToFin(new IngressFault.InvalidEnvelope(None, "<absent-operation-id>"))
            .Bind(id => EventId.Admit(id, key));

    // Payload shape is admitted at ONE site and the unsupported arm is a typed refusal, never a throw the
    // enclosing frame had to catch straight back into the value it should have been.
    static Fin<IngressPayload> Payload(
        CloudEvent envelope, global::Rasm.Contracts.Event.Extensions extensions, string id, Op key) =>
        key.Catch(() => {
            Option<Uri> reference = extensions.HasDataref
                ? Some(new Uri(extensions.Dataref, UriKind.RelativeOrAbsolute))
                : None;
            return envelope.Data switch {
                byte[] bytes => Fin.Succ<IngressPayload>(new IngressPayload.Inline(bytes)),
                ReadOnlyMemory<byte> memory => Fin.Succ<IngressPayload>(new IngressPayload.Inline(memory)),
                Memory<byte> memory => Fin.Succ<IngressPayload>(new IngressPayload.Inline(memory)),
                Stream stream => Fin.Succ<IngressPayload>(new IngressPayload.Inline(BinaryDataUtilities.ToReadOnlyMemory(stream))),
                null => reference.Map<IngressPayload>(static held => new IngressPayload.External(held))
                    .ToFin(new IngressFault.InvalidEnvelope(Some(id), "<absent-data-and-dataref>")),
                _ => Fin<IngressPayload>.Fail(new IngressFault.InvalidEnvelope(Some(id), "<unsupported-cloudevent-data>")),
            };
        }).MapFail(error => IngressFault.Lift(error,
                static raised => raised is IOException or UriFormatException,
                cause => new IngressFault.EnvelopeRejected(Some(id), cause))));

    // Apply rides the same funnel: `TryApply` states its refusal as a `Fin`, but the `RunAsync`
    // boundary itself can throw and the bare `IsFail` read past it never covered that. The winner/duplicate
    // discrimination is the store's own conditional insert, never a read-then-write probe.
    static async ValueTask<Fin<IngressOutcome>> Applied(AdmittedRecord admitted, IngressPorts ports, string topic,
        Op key, CancellationToken token) {
        Fin<ReadOnlyMemory<byte>> acquired = await Acquire(admitted.Payload, ports, token).RunAsync(token).ConfigureAwait(false);
        Fin<bool> outcome = await acquired.Bind(payload => Verified(payload, admitted.ContentKey, admitted.Key.Id))
            .Match(
                Succ: payload => key.Catch(() => ports.TryApply(
                    admitted.Key, admitted.ContentKey, admitted.Envelope, admitted.Extensions, payload, token).RunAsync(token)),
                Fail: error => ValueTask.FromResult(Fin<bool>.Fail(error)))
            .ConfigureAwait(false);
        return await outcome
            .Match(Succ: won => ValueTask.FromResult(Fin.Succ(won ? IngressOutcome.Applied : IngressOutcome.Duplicate)),
                   Fail: fault => Dead(ports, fault, topic, token))
            .ConfigureAwait(false);
    }

    static IO<Fin<ReadOnlyMemory<byte>>> Acquire(IngressPayload payload, IngressPorts ports, CancellationToken token) =>
        payload.Switch(
            inline: row => IO.pure(Fin.Succ(row.Bytes)),
            external: row => ports.Resolve(row.Reference, token));

    static Fin<ReadOnlyMemory<byte>> Verified(ReadOnlyMemory<byte> payload, UInt128 expected, string id) =>
        ContentHash.Of(payload.Span) == expected
            ? Fin.Succ(payload)
            : Fin.Fail<ReadOnlyMemory<byte>>(new IngressFault.InvalidEnvelope(Some(id), "<payload-content-key-mismatch>"));

    // Refusal exits at ONE site: a classified ingress refusal derives its routing key from its case; every foreign,
    // cancellation, or already-typed error rails out exact and never becomes a dead letter this owner cannot name.
    static async ValueTask<Fin<IngressOutcome>> Dead(IngressPorts ports, Error fault, string topic, CancellationToken token) {
        if (fault is not IngressFault banded) { return Fin<IngressOutcome>.Fail(fault); }
        await ports.DeadLetter(ports.ObserveFault(banded), banded.Route(topic), token).RunAsync(token).ConfigureAwait(false);
        return Fin.Succ(IngressOutcome.DeadLettered);
    }
}
```

| [INDEX] | [POLICY]         | [VALUE]                                            | [BINDING]                                                   |
| :-----: | :--------------- | :------------------------------------------------- | :---------------------------------------------------------- |
|  [01]   | consume seam     | instrumented builder twin, flags explicit          | `AddKafkaConsumerInstrumentation` rides the AppHost root    |
|  [02]   | context join     | instrumented wrapper extraction                    | one receive/process span pair wraps every record            |
|  [03]   | dedup identity   | the `(source, id)` composite vs the op-log index   | `id` alone merges two producers' unrelated operations       |
|  [04]   | offset law       | `StoreOffset` after settle, batch `Commit`         | auto-commit disabled; the cursor never outruns the store    |
|  [05]   | source gate      | admitted message envelope source set               | a mixed topic cannot leak an unvetted op onto the rail      |
|  [06]   | family split     | ingress owner beside the egress sink family        | the binding roster stays egress-only; no shared row         |
|  [07]   | resume origin    | the row's `ResumeOrigin` case                      | a fresh group's start position is a row, not a default      |
|  [08]   | resolve shape    | ONE `OffsetsForTimes` over the whole assignment    | chunking multiplies the round trip AND the timeout          |
|  [09]   | resolve budget   | `PollInterval` over `ResolveHeadroom`              | the poll thread's own share, never a bare literal           |
|  [10]   | resolve refusal  | `Local_TimedOut` folds to `Offset.Stored`          | a handler throw re-raises out of `Consume`                  |
|  [11]   | edge evidence    | armed `EnablePartitionEof`, off the wrapper RETURN | the callback never sees an edge; the return is its producer |
|  [12]   | bound            | `Pause`/`Resume` over the held assignment          | fetch halts; the assignment survives the session timeout    |
|  [13]   | teardown         | `Close`, never a bare `Dispose`                    | `Dispose` commits nothing and strands the group to timeout  |
|  [14]   | refusal shape    | `ConsumeException` throws, `Error` callbacks       | the settle fold reads both; neither rail sees the other     |
|  [15]   | decode fields    | descriptor correspondence + Celly, both ways       | a caller-supplied array drifts from the producer contract   |
|  [16]   | content key      | `ContentHash.Admit` round trip over `subject`      | a bare parse collapses upper-case and short hex spellings   |
|  [17]   | throw crossing   | one `Op.Catch` into `IngressFault.Lift`            | only documented exception families mint stage cases         |
|  [18]   | cancellation     | `KernelFault.Cancelled` passes `Lift` unbanded     | a stopped drain is not a malformed record                   |
|  [19]   | fault codes      | `[FaultCase]` on the kernel `Fault` floor          | contiguous declaration offsets 0-5                          |
|  [20]   | admission        | one `Admit` chain, one dead-letter exit            | per-step refusal returns became one                         |
|  [21]   | settlement       | `IngressOutcome` rows carry their tally step       | conservation is structural, not re-derived per site         |
|  [22]   | payload carriage | `Inline \| External` then key verification         | `dataref` is resolved, never treated as a missing body      |

## [03]-[RESEARCH]

(none)

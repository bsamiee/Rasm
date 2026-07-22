# [PERSISTENCE_VERSION_INGRESS]

`CdcIngress` drains foreign broker topics onto the durable rail — the consume half of the CDC boundary whose egress half `Version/egress` owns as a sink-only family. Foreign Kafka records admit through the instrumented consumer twins, decode into the one CloudEvents envelope vocabulary, dedup by the envelope `id` content key against the op-log, continue W3C context off message headers, and fold onto the changefeed as first-class ops. Offsets commit only after durable apply, so the broker cursor never outruns the store.

## [01]-[INDEX]

- [01]-[INGRESS_PUMP]: the instrumented consume leg, the envelope decode and source gate, the content-key dedup against the op-log, the durable apply-then-commit law, the `IngressReceipt` conservation fold, and the 8500 fault band.

## [02]-[INGRESS_PUMP]

- Owner: `CdcIngress` the static surface owning the one consume fold — instrumented consume, context join, envelope decode, source gate, atomic content-key apply, offset commit; `IngressSource` the foreign-topic binding row (topic, group, admitted source URI set, batch width) the composition root fills; `IngressPorts` the injected delegate frame (formatter, atomic apply arrow, dead-letter arrow) so no provider type crosses into the fold; `IngressReceipt` the per-drain evidence implementing the kernel `IValidityEvidence`; `IngressFault` the 8500 band registered as the `FaultBand.Ingress` row.
- Cases: a consumed record decodes through the shared `JsonEventFormatter` into a `CloudEvent`, and four outcomes close the fold — `Applied` (the atomic content-key apply inserted and folded a fresh op, offset stored), `Duplicate` (the same conditional statement observed an existing envelope `id`, skipped and stored — the replay the `Version/egress#EGRESS_SINK` dedup-honesty column promises every producer absorbs), `ForeignSource` (an envelope `source` outside the admitted set, dead-lettered), `Refused` (a decode or apply refusal, dead-lettered) — so `consumed == applied + duplicates + deadLettered` is the receipt's conservation law.
- Entry: `Consume(IConsumer<string, byte[]>, IngressSource, IngressPorts, ProjectionContext, CancellationToken)` admits a positive batch, threads cancellation through the wrapped record settlement, and stores one offset after settlement; `Bind` constructs and subscribes the instrumented consumer.
- Auto: the consumer disables `EnableAutoCommit` and commits through `StoreOffset(consumeResult)` and explicit `Commit` only after each outcome settles durably, so a crash between apply and commit re-consumes and content-key dedup absorbs the replay; dedup identity is the envelope `id`, passed into one `TryApply` arrow whose store-owned conditional insert and op fold share one transaction and return `true` only for the winning insert; the admitted-source gate reads the envelope `source` before apply; the wrapper's processing activity continues the foreign W3C context across the durable apply.
- Receipt: a consume batch rides `store.ingress.consume` carrying the topic, group, consumed/applied/duplicate/dead-lettered counts, and elapsed duration; a dead-letter rides `store.ingress.deadletter` carrying the envelope id and the fault.
- Packages: Confluent.Kafka (`ConsumerBuilder<TKey,TValue>`/`ConsumerConfig`/`IConsumer<TKey,TValue>`/`ConsumeResult<TKey,TValue>`/`StoreOffset`/`Commit`/`Subscribe`), OpenTelemetry.Instrumentation.ConfluentKafka (`AsInstrumentedConsumerBuilder` + `ConfluentKafkaInstrumentedConsumerBuilderOptions` at the bind seam; `ConsumeAndProcessMessageAsync` owns context extraction and the receive/process spans; `AddKafkaConsumerInstrumentation` registers at the AppHost root), CloudNative.CloudEvents (+`.Kafka` `ToCloudEvent`/`IsCloudEvent`, +`.SystemTextJson` `JsonEventFormatter`), Rasm (`IValidityEvidence`/`ValidityClaim`), Rasm.Persistence (`Element/graph#FAULT_TABLES` `FaultBand`, `Version/ledger` `OpLogEntry`), LanguageExt.Core, NodaTime, Thinktecture.Runtime.Extensions, BCL inbox.
- Growth: a new foreign topic is one `IngressSource` row at composition — zero fold edits; a new admission predicate is one gate row on the source binding; a new ingress transport is one sibling consume leg over the same ports frame, never a second dedup or apply path; zero new surface — an ingress case on the egress `EgressSink` family, an auto-committed consumer, a per-topic apply fold, a dedup keyed on broker offset instead of content identity, or a raw `ConsumeResult` crossing into the apply arrow is the deleted form because the sink family stays egress-only, the offset law is store-first, and the envelope `id` is the one replay identity.
- Boundary: the `EgressSink` family stays egress-only; consumer construction is Persistence's `Bind` seam while instrumentation registration rides the AppHost root; `TryApply` hands the content key and admitted op to the changefeed owner as one atomic conditional operation, so no read-then-write dedup window exists; `IngressPorts.Extensions` feeds the typed `ToCloudEvent` overload; binary data folds through the CloudEvents byte glue; the processing token reaches every port effect and gates offset storage and batch commit.

```csharp signature
// --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
using CloudNative.CloudEvents;
using CloudNative.CloudEvents.Core;
using CloudNative.CloudEvents.Kafka;
using CloudNative.CloudEvents.SystemTextJson;
using Confluent.Kafka;
using Rasm.Persistence.Element;                   // FaultBand — the one band registry (graph#FAULT_TABLES)

namespace Rasm.Persistence.Version;

// --- [MODELS] ---------------------------------------------------------------------------

// foreign-topic binding row the composition root fills: the broker subscription identity, the admitted
// envelope-source set the gate reads, and the bounded batch width one `Consume` drains.
public sealed record IngressSource(string Topic, string Group, FrozenSet<string> Admitted, int Batch);

// injected delegate frame: `TryApply` atomically claims the envelope id and folds an admitted foreign op
// onto the changefeed, returning true for the winner and false for a duplicate; `DeadLetter` persists the
// refused record — values on a Persistence-owned shape, never a provider type inside the fold.
public sealed record IngressPorts(
    CloudEventFormatter Formatter,
    CloudEventAttribute[] Extensions,
    Func<UInt128, CloudEvent, ReadOnlyMemory<byte>, CancellationToken, IO<Fin<bool>>> TryApply,
    Func<string, string, CancellationToken, IO<Unit>> DeadLetter);

// Per-drain evidence on the kernel validity floor: every consumed record is applied, deduplicated, or
// dead-lettered, exactly once.
public sealed record IngressReceipt(string Topic, string Group, int Consumed, int Applied, int Duplicates, int DeadLettered, Duration Elapsed, Instant At, Guid Correlation) : IValidityEvidence {
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

    // one construction seam: the plain builder lifts into the instrumented twin with both flags explicit
    // (off-by-default outside DI), auto-commit disabled so the offset law stays store-first.
    public static IConsumer<string, byte[]> Bind(ConsumerConfig config, IngressSource source) {
        config.GroupId = source.Group;
        config.EnableAutoCommit = false;
        IConsumer<string, byte[]> consumer = new ConsumerBuilder<string, byte[]>(config)
            .AsInstrumentedConsumerBuilder(new ConfluentKafkaInstrumentedConsumerBuilderOptions { EnableTraces = true, EnableMetrics = true })
            .Build();
        consumer.Subscribe(source.Topic);
        return consumer;
    }

    // ONE consume fold per record inside the wrapped receive/process span pair: context join -> envelope
    // decode -> source gate -> content-key dedup -> durable apply -> StoreOffset. Commit lands once per batch
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
            (Applied: 0, Duplicates: 0, Dead: 0, Consumed: 0),
            (state, _) => IO.liftAsync(async () => {
                (int applied, int duplicates, int dead, int consumed) = (0, 0, 0, 0);
                await consumer.ConsumeAndProcessMessageAsync(async (result, _, processingToken) => {
                    consumed = 1;
                    (applied, duplicates, dead) = await Settle(result, source, ports, processingToken).ConfigureAwait(false);
                    processingToken.ThrowIfCancellationRequested();
                    consumer.StoreOffset(result);
                }, token).ConfigureAwait(false);
                return (state.Applied + applied, state.Duplicates + duplicates, state.Dead + dead, state.Consumed + consumed);
            })).As()
        from committed in IO.lift(() => {
            token.ThrowIfCancellationRequested();
            try { consumer.Commit(); return Fin<Unit>.Succ(unit); }
            catch (Exception failure) { return Fin<Unit>.Fail(new IngressFault.CommitRegressed(source.Topic, failure.Message)); }
        })
        select committed.Map(_ => new IngressReceipt(
            source.Topic, source.Group, folded.Consumed, folded.Applied, folded.Duplicates, folded.Dead,
            frame.Elapsed(mark), frame.Now(), frame.Correlation));

    static async ValueTask<(int Applied, int Duplicates, int Dead)> Settle(ConsumeResult<string, byte[]> result,
        IngressSource source, IngressPorts ports, CancellationToken token) {
        token.ThrowIfCancellationRequested();
        if (!result.Message.IsCloudEvent()) return await Dead(ports, "<not-cloudevent>", result.Topic, token).ConfigureAwait(false);

        CloudEvent envelope;
        try { envelope = result.Message.ToCloudEvent(ports.Formatter, ports.Extensions); }
        catch (OperationCanceledException) when (token.IsCancellationRequested) { throw; }
        catch (Exception failure) {
            return await Dead(ports, new IngressFault.EnvelopeRejected(failure.Message).Message, result.Topic, token).ConfigureAwait(false);
        }

        if (envelope.Source is not { } origin || !source.Admitted.Contains(origin.ToString()))
            return await Dead(ports, new IngressFault.ForeignSource(envelope.Source?.ToString() ?? "<none>").Message,
                envelope.Id ?? "<none>", token).ConfigureAwait(false);

        if (envelope.Id is not { } envelopeId || !UInt128.TryParse(envelopeId, System.Globalization.NumberStyles.HexNumber,
                System.Globalization.CultureInfo.InvariantCulture, out UInt128 contentKey))
            return await Dead(ports, new IngressFault.EnvelopeRejected("<invalid-content-key>").Message,
                envelope.Id ?? "<none>", token).ConfigureAwait(false);

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

        Fin<bool> outcome = await ports.TryApply(contentKey, envelope, payload, token).RunAsync(token).ConfigureAwait(false);
        return outcome.IsFail
            ? await Dead(ports, new IngressFault.ApplyRefused(envelopeId, outcome.Error.Message).Message, envelopeId, token).ConfigureAwait(false)
            : outcome.ValueUnsafe() ? (1, 0, 0) : (0, 1, 0);
    }

    static async ValueTask<(int Applied, int Duplicates, int Dead)> Dead(IngressPorts ports, string fault, string key,
        CancellationToken token) {
        await ports.DeadLetter(fault, key, token).RunAsync(token).ConfigureAwait(false);
        return (0, 0, 1);
    }
}
```

| [INDEX] | [POLICY]       | [VALUE]                                       | [BINDING]                                                |
| :-----: | :------------- | :-------------------------------------------- | :------------------------------------------------------- |
|  [01]   | consume seam   | instrumented builder twin, flags explicit     | `AddKafkaConsumerInstrumentation` rides the AppHost root |
|  [02]   | context join   | instrumented wrapper extraction               | one receive/process span pair wraps every record         |
|  [03]   | dedup identity | envelope `id` content key vs the op-log index | broker offset never keys replay identity                 |
|  [04]   | offset law     | `StoreOffset` after settle, batch `Commit`    | auto-commit disabled; the cursor never outruns the store |
|  [05]   | source gate    | admitted envelope-source set                  | a mixed topic cannot leak an unvetted op onto the rail   |
|  [06]   | family split   | ingress owner beside the egress sink family   | `EgressSink` stays egress-only; no shared case           |

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)

# [PERSISTENCE_VERSION_EGRESS]

`EgressPump` drains durable `OpLogEntry` rows past each subscription cursor, mints one envelope through the branch owner, folds every provider result into `DeliveryAck`, and advances only a confirmed contiguous prefix. `Subscription` instances ride `Binding` rows carrying modes, prefix, routing member, `dataref` policy, and `protocolsettings` roster as DATA, so a delivery target is a value a deployment authors. `(source, id)` keys replay beside the content-keyed `subject`; presence and awareness never enter this durable rail.

## [01]-[INDEX]

- [02]-[EGRESS_PUMP]: `EgressPump` drains one fold past each subscription cursor — profile lane gate, advance law, dead-letter and replay rows, `EgressReceipt` floor, 8270 band.
- [03]-[EGRESS_SINK]: `Egress.Envelope` mints the envelope every `Subscription` delivers, the `Binding` roster and its `ProtocolSettings` admission, and the `DeliveryAck` fold beside its `KafkaAck` boundary owner under the dedup, settlement-contract, retry-owner, and in-flight-bound columns.
- [04]-[SUBSCRIPTION_FILTER]: `FilterDialect` the seven-dialect delivery predicate, `Cesql` the table-driven expression owner, and the accumulating `CesqlFault` rail its evaluation returns.

## [02]-[EGRESS_PUMP]

- Owner: `EgressPump` owns feed, drain, envelope mint, delivery, acknowledgement, replay, and flushing close. `DrainLane` is one bounded subscription channel. `DeadLetterRow` is the typed quarantine document; Store coordination owns the sole `QuarantineAndAdvance` transaction. `EgressReceipt`, `EgressFault`, and `EgressPorts` own evidence, refusal, and composition.
- Entry: `Lane` is the profile token; `Partition` publishes the quarantine mapping; `Offer`, `Drain`, and `Close` own the bounded delivery lifecycle. Store coordination's `Coordinate.QuarantineAndAdvance` stores the letter only after the fenced advance verdict succeeds, then commits both through the same session. `EgressPorts.QuarantineAndAdvance` is the composition-bound arrow AppHost maps. `Replay` reuses the delivery fold; `Reletter` updates only an already-advanced row.
- Auto: the feed half and the delivery half meet on ONE bounded lane per subscription, so a lagging sink backpressures its own reader rather than stalling the shared feed or buffering the outbox in memory, and the fold hands rows to each leg in sequence order so a mid-batch refusal never advances past unconfirmed work. Rows the subscription's own filters withhold settle as delivered-and-filtered rather than as an ack, because a predicate answering false is a routing decision the receipt counts and never a transport outcome; an envelope the branch owner REFUSES to mint is poison by construction and letters, since a malformed grammar value cannot become well-formed on a later attempt. Wake arrives on the coordination `pg_notify` channel through `NpgsqlConnection.WaitAsync`, with the bounded poll as the correctness FLOOR — a missed NOTIFY costs latency, never a lost row, because the cursor law owns correctness.
- Auto: what a binding preserves is its own engine's answer, not the envelope's — `partitionkey` reaches a real routing key on Kafka (`Message.Key`) and Pulsar (`MessageMetadata.Key`/`OrderingKey`), while NATS orders per subject, RabbitMQ per queue, MQTT per topic, and AMQP per link, none of which expose a key member at all, so per-entity order on those rows holds only where one entity's rows share one subject, queue, topic, or link (`#EGRESS_SINK`) and a blanket per-entity-order claim over the whole family reads as a guarantee those engines never made. Row `http` reconciles `DeliveryUnconfirmed` by re-reading `net._http_response` by request-id on the NEXT drain, so a PENDING response resolves without a dedicated poller; a crash between delivery and advance re-drains the suffix and every binding's dedup column states what absorbs it. Dead-letter replay decrements nothing: the conservation fold proves `delivered + filtered + held + deadLettered == drained` on every drain.
- Receipt: a drain rides `store.egress.drain` carrying the sink, the from/through sequences, and the delivered/duplicate/held/dead-lettered counts; a dead-letter rides `store.egress.deadletter` carrying the content key and the fault; a replay rides `store.egress.replay`; each settled drain receipt fires the `rasm.persistence.egress.delivered` observe point (`Store/observability#HOOK_RAIL`) as a composition-root tap on the drain outcome, never an emit call inside the fold.
- Packages: Npgsql (`NpgsqlConnection.Notification`/`WaitAsync` — the pump wake), Marten (`IDocumentSession.Store`/`SaveChangesAsync` — the dead-letter document; `StoreOptions.Schema.For<T>().PartitionOn` through `RollingWindow.Declare` — its rolling window), Rasm.Contracts (`Fault.FaultObservation`), Rasm (`IValidityEvidence`/`ValidityClaim`), Microsoft.Extensions.Compliance.Redaction (`IRedactorProvider.GetRedactor(DataClassificationSet)` — the classified-field gate before the boundary), LanguageExt.Core, NodaTime, Thinktecture.Runtime.Extensions, BCL inbox.
- Growth: a new delivery target is one `Subscription` value over an existing `Binding` row and one `outbox_cursor` row minted on first drain — zero pump edits and zero new types; a new transport is one `Binding` row carrying its modes, prefix, routing member, `protocolsettings` roster, and `dataref` policy; a new drain policy (batch width, wake channel, payload arrow) is one `EgressPorts`/subscription value; zero new surface — a per-sink pump, a second delivery path for replay, a fire-and-forget HTTP post, a presence row in the CDC drain, a lane gate seated at a caller instead of these two entries, or a CDC poller beside the changefeed is the deleted form because the pump is one fold, replay is the same fold, the advance law owns the cursor, and the durable lanes are the only drain source.
- Boundary: the pump drains only durable rows. `Wait` is the sole full mode and close flushes. Delivered prefixes use `OutboxAdvance`; a first terminal row uses only `QuarantineAndAdvance`, never `DeadLetter` followed by `OutboxAdvance`. Replay may `Reletter` because its cursor was advanced by the first terminal commit. Cursor sequence and the CloudEvents `D20` `OutboxOrdinal` are the same non-negative store-local position; neither is an HLC or portable causal coordinate.
- Boundary: the payload arrow redacts and frames BEFORE the mint (`ErasingRedactor` the fail-closed fallback), so an out-of-authority payload crosses masked rather than raw and the grade it answers is the `dataclassification` the mint stamps. Caller cancellation passes through untyped; the wire-native row hands bytes to the AppHost `OutboundHop` keyed pipeline and reads its delivery-honesty policy, so Persistence never owns that channel. Letters retire by PARTITION DROP, not by row sweep, so a letter neither `Retire` nor `Replay` ever consumed leaves at its window's trailing edge as one receipted `Version/retention#SWEEP_AND_GC` `DropPartition` and an unbounded letter table has no reachable state.

```csharp signature
// --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
using Rasm.Domain;                                // CorrelationId — the S0 causal half the frame seats
using Rasm.Persistence.Element;                   // FaultBand — the one band registry (Rasm/Domain/rails#FAULT_BAND)
using Rasm.Persistence.Store;                     // OutboxCursor / SinkKey / OutboxAdvance (coordination#OUTBOX_CURSOR); RollingWindow / StoreProfile (provisioning#SERVER_EXTENSIONS)
using System.Threading.Channels;                  // DrainLane, the fan-out lane; and the AMQP leg's own in-flight bound

namespace Rasm.Persistence.Version;

// --- [MODELS] ---------------------------------------------------------------------------

// Stored as a Marten document in the SAME session as the cursor state and replayed by content key through the one
// pump fold. The row admits under the `Version/retention#RETENTION_CLASSES` `evidence` class, and `Window` projects
// `At` onto the duplicated stamp the `RollingWindow` `dead-letter` row partitions on — one derived member, never a
// second stamp beside `At` — so a letter no `Retire` consumed leaves with its partition as one receipted
// `DropPartition` rather than accumulating past every attempt the schedule admits.
public sealed record DeadLetterRow(
    UInt128 ContentKey,
    SinkKey Sink,
    long Sequence,
    global::Rasm.Contracts.Fault.FaultObservation Fault,
    int Attempts,
    Instant At) {
    // Sink plus operation content gives retries one stable document identity. Ambient UUID minting duplicates a
    // quarantine row after an ambiguous committed response, while content alone aliases two subscription cursors.
    public Guid Id { get; init; } = new(ContentHash.Wire(ContentHash.Of(
        (Sink: Sink.Value, Key: ContentKey),
        static (state, writer) => writer.String(state.Sink).U128(state.Key))).Span);
    public DateTimeOffset Window => At.ToDateTimeOffset();
}

// Payload framing answers WHOLE, minted once per row: the framed body, the grade the redaction pass settled, the
// serdes arrow's OWN content type, its optional absolute data-schema URI, the EVENT-semantic major, and the complete
// generated extension contribution. Registry subject/version stays serde configuration; an `Any` type URL stays
// protobuf payload configuration. Neither aliases the CloudEvents `dataschema` attribute or the event-type major.
// ONE arrow owns these fields because only the pass framing the bytes can keep them aligned. `Residence` fills only
// where the drain externalizes.
public readonly record struct PayloadFrame(
    ReadOnlyMemory<byte> Body,
    DataGrade Grade,
    string ContentType,
    Option<Uri> DataSchema,
    int EventMajor,
    global::Rasm.Contracts.Event.Extensions Extensions,
    Option<Uri> Residence = default);

// Changefeed fan-out rides this lane: one bounded channel per subscription between the feed half and the delivery
// half, so a slow sink backpressures its OWN lane rather than stalling the shared feed or growing an unbounded queue.
// `Wait` is the ONE admissible full mode on a durable plane — every drop mode discards a row the conservation fold
// carries no arm to express — and the close is a FLUSH: the writer completes and the reader drains to `Completion`,
// so shutdown sheds nothing. The owner retains the WRITER and publishes only the reader, so a delivery leg can
// neither write nor complete the lane it drains.
public sealed record DrainLane(SinkKey Sink, Channel<OpLogEntry> Rows) {
    public static DrainLane Of(SinkKey sink, Dimension depth) =>
        new(sink, Channel.CreateBounded<OpLogEntry>(new BoundedChannelOptions(depth.Value) {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = true,
        }));

    public ChannelReader<OpLogEntry> Reader => Rows.Reader;
    // Observable gauges pull this buffered level at collection cadence, read ONLY where the channel counts — `Count`
    // throws on a channel that does not.
    public Option<int> Depth => Rows.Reader.CanCount ? Some(Rows.Reader.Count) : None;

    public IO<Unit> Publish(OpLogEntry row) =>
        IO.liftAsync(async env => await Op.Of().Catch(async token => {
            await Rows.Writer.WriteAsync(row, token).ConfigureAwait(false);
            return Fin<Unit>.Succ(unit);
        }, env.Token).ConfigureAwait(false)).Bind(IO.liftFin);

    // `TryComplete` is the idempotent form, so a second close answers false rather than throwing at a shutdown path
    // that cannot rail.
    public IO<Unit> Flush() => IO.lift(() => { Rows.Writer.TryComplete(); return unit; });
}

// Composition fills this injected delegate frame, all values on a Persistence-owned shape and never an AppHost type.
// `Profile` seats first because every entry reads it before any other member. `QuarantineAndAdvance` closes over
// the session, receipt sink, frame, and cancellation token needed by the public operation below; AppHost maps its
// one terminal delegate to that arrow. `Reletter` serves only replay of a row whose cursor already advanced.
// `Reside` is the required object-store adapter: it converts
// the storage result into the URI-reference a receiver resolves, and composition refuses without that address.
// `Carrier` renders a continued
// context onto the KERNEL carrier at the propagator owning that format, because a `$"00-{traceId}-{spanId}-01"`
// interpolation here freezes version and flag byte against a spec revision the propagator tracks. `ObserveFault`
// is the injected AppHost `FaultWire.Observe` projection: Persistence stores the generated observation and never
// recreates its recovery, cause, or family grammar.
public sealed record EgressPorts(
    StoreProfile Profile,
    Func<IO<Unit>> Wait,
    Func<CoordinationOp, Option<LeaseToken>, IO<Fin<CoordinationReceipt>>> Coordinate,
    Func<ReplayWindow, IO<Seq<OpLogEntry>>> Feed,
    Func<OpLogEntry, PayloadFrame> Frame,
    Func<PayloadFrame, IO<Uri>> Reside,
    Func<ActivityContext, TraceCarrier> Carrier,
    Func<Error, global::Rasm.Contracts.Fault.FaultObservation> ObserveFault,
    Func<SinkKey, int, IO<Seq<DeadLetterRow>>> Letters,
    Func<DeadLetterRow, Option<LeaseToken>, IO<Fin<OutboxCursor>>> QuarantineAndAdvance,
    Func<DeadLetterRow, IO<Unit>> Reletter,
    Func<DeadLetterRow, IO<Unit>> Retire);

// Cursor drains and cursor-free replay are disjoint receipt coordinates; the closed shape makes the impossible
// half-present `(from, through)` pair unrepresentable and replay never fabricates a zero cursor.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record EgressWindow {
    private EgressWindow() { }
    public sealed record Cursor(long From, long Through) : EgressWindow;
    public sealed record Replay : EgressWindow;
}

// `EgressReceipt` rides the kernel validity floor as per-drain evidence: conservation is the fold — every drained row
// is delivered, filtered, held, or dead-lettered, exactly once. `Filtered` is a DELIVERY-SIDE verdict the
// subscription's own predicate reached, so it settles the row and advances the cursor exactly as a `Persisted` does;
// folding it into `Delivered` would report traffic to a transport that received nothing.
public sealed record EgressReceipt(SinkKey Sink, EgressWindow Window, int Drained, int Delivered, int Duplicates,
    int Filtered, int Held, int DeadLettered, Duration Elapsed, Instant At, CorrelationId Correlation) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        Window.Switch(cursor: static row => row.Through >= row.From, replay: static _ => true),
        ValidityClaim.CountExactly(Delivered + Filtered + Held + DeadLettered, Drained),
        ValidityClaim.CountAtLeast(Delivered, Duplicates));
}

// --- [ERRORS] ---------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record EgressFault : Fault {
    private static readonly FaultBand FamilyBand = FaultBand.Egress;
    private EgressFault() { }

    [FaultCase(0)]
    public sealed partial record DeadLetter(UInt128 ContentKey, SinkKey Sink, Error Cause) : EgressFault(), ICausedFault;
    [FaultCase(1)]
    public sealed partial record SinkRefused(SinkKey Sink, string Detail) : EgressFault();
    [FaultCase(2)]
    public sealed partial record CursorStall(SinkKey Sink, long Held) : EgressFault();
    [FaultCase(3)]
    public sealed partial record DeliveryUnconfirmed(SinkKey Sink, long RequestId) : EgressFault();
    [FaultCase(4)]
    public sealed partial record LaneUnrealizable(SinkKey Sink, string Lane) : EgressFault();

    // Admission-side refusal beside the four delivery outcomes: a `protocolsettings` slice missing a required key or
    // carrying one the binding row never declares refuses the SUBSCRIPTION, so a governance value no leg reads can
    // never reach a deployment that believes it configured something.
    [FaultCase(5)]
    public sealed partial record SettingsRejected(string Binding, string Detail, Option<Op> Key = default) : EgressFault();

    public override string Message => Switch(
        deadLetter:          static c => $"<dead-letter:{c.Sink.Value}:{c.ContentKey:x32}>:{c.Cause.Message}",
        sinkRefused:         static c => $"<sink-refused:{c.Sink.Value}>:{c.Detail}",
        cursorStall:         static c => $"<cursor-stall:{c.Sink.Value}@{c.Held}>",
        deliveryUnconfirmed: static c => $"<delivery-unconfirmed:{c.Sink.Value}#{c.RequestId}>",
        laneUnrealizable:    static c => $"<lane-unrealizable:{c.Sink.Value}:{c.Lane}>",
        settingsRejected:    static c => $"<settings-rejected:{c.Binding}>:{c.Detail}");
}

// --- [BOUNDARIES] -----------------------------------------------------------------------

// --- [OPERATIONS] -----------------------------------------------------------------------

public static class EgressPump {
    public static readonly StoreSlot DrainSlot = StoreSlot.Create("store.egress.drain");
    public static readonly StoreSlot DeadLetterSlot = StoreSlot.Create("store.egress.deadletter");
    public static readonly StoreSlot ReplaySlot = StoreSlot.Create("store.egress.replay");
    public static readonly Seq<StoreSlot> Slots = Seq(DrainSlot, DeadLetterSlot, ReplaySlot);

    // Lane token this pump admits under, spelled to match its `Store/provisioning#SERVER_EXTENSIONS`
    // `StoreProfile.Lanes` member byte for byte — that set compares ORDINALLY, so a case or spacing drift here reads
    // as an absent lane and refuses every drain on a server profile that realizes it perfectly well.
    public const string Lane = "egress";

    // ONE refusal for both entries: an unrealizable lane is an admission verdict with no drain behind it, so it
    // answers before the wake and carries no from/through pair a caller could mistake for an empty window.
    static IO<Fin<T>> Unrealizable<T>(SinkKey sink) =>
        IO.pure(Fin<T>.Fail(new EgressFault.LaneUnrealizable(sink, Lane)));

    // `Partition` publishes the letter table's mapping contribution the composition root folds over the
    // `Element/graph#STREAM_GRAIN` spine seat: the policy is the `Store/provisioning#SERVER_EXTENSIONS`
    // `RollingWindow` `dead-letter` row and this publishes only the duplicated key, so a `PartitionOn` carrying its
    // own period literals is the deleted form. The contribution seats HERE because the S0 spine may not name this
    // document type.
    public static StoreOptions Partition(StoreOptions opts) =>
        RollingWindow.DeadLetter.Declare<DeadLetterRow>(opts, static row => row.Window);

    // Feeding takes this half: wake, read the window, and publish every row onto the sink's own lane. `WriteAsync`
    // AWAITS capacity, so a lagging delivery leg throttles this read instead of buffering the outbox in memory,
    // and the answer is the sequence offered rather than a count no advance law can bound itself against.
    public static IO<Fin<long>> Offer(Subscription sink, OutboxCursor cursor, DrainLane lane, EgressPorts ports) =>
        ports.Profile.Admits(Lane)
            ? from _ in ports.Wait()
              from rows in ports.Feed(ReplayWindow.DurableOps(cursor.Sequence, sink.Bind.Batch.Value))
              from __ in rows.TraverseM(lane.Publish).As()
              select Fin<long>.Succ(rows.Last.Map(static row => row.Sequence).IfNone(cursor.Sequence))
            : IO.pure(Fin<long>.Fail(new EgressFault.LaneUnrealizable(sink.Bind.Key, Lane)));

    // ONE drain fold over the lane's READER: dequeued rows -> envelope mint -> subscription filter -> delivery ->
    // DeliveryAck -> contiguous-prefix cursor advance. The first Indeterminate holds the cursor. A refused row
    // crosses the one atomic quarantine-and-advance arrow before counting; no separately callable letter write can
    // strand a poison row behind the cursor or advance without its evidence.
    public static IO<Fin<EgressReceipt>> Drain(Subscription sink, OutboxCursor cursor, DrainLane lane, EgressPorts ports, ProjectionContext frame) =>
        ports.Profile.Admits(Lane)
            ? Drained(sink, cursor, lane, ports, frame)
            : Unrealizable<EgressReceipt>(sink.Bind.Key);

    // Never-shedding planes close by FLUSHING: the writer completes and the reader drains what it already holds, so
    // shutdown delivers the lane rather than dropping it — the arm a drop full-mode would make unnecessary and the
    // conservation fold unprovable.
    public static IO<Fin<EgressReceipt>> Close(Subscription sink, OutboxCursor cursor, DrainLane lane, EgressPorts ports, ProjectionContext frame) =>
        ports.Profile.Admits(Lane)
            ? Closed(sink, cursor, lane, ports, frame)
            : Unrealizable<EgressReceipt>(sink.Bind.Key);

    static IO<Fin<EgressReceipt>> Closed(
        Subscription sink, OutboxCursor cursor, DrainLane lane, EgressPorts ports, ProjectionContext frame) =>
        from _ in lane.Flush()
        from rows in IO.lift(() => Take(lane, int.MaxValue))
        from receipt in Settled(sink, cursor, rows, ports, frame)
        select receipt;

    // NON-BLOCKING dequeue takes the `EXPRESSION_SPINE` named-kernel exemption: a normal drain takes its declared
    // window; close completes the writer first and passes `int.MaxValue`, so the channel's own capacity bounds the
    // terminal set and the first empty probe is final rather than a race with a future offer.
    static Seq<OpLogEntry> Take(DrainLane lane, int batch) {
        Seq<OpLogEntry> held = Seq<OpLogEntry>();
        while (held.Count < batch && lane.Reader.TryRead(out OpLogEntry? row) && row is { } value) { held = held.Add(value); }
        return held;
    }

    static IO<Fin<EgressReceipt>> Drained(Subscription sink, OutboxCursor cursor, DrainLane lane, EgressPorts ports, ProjectionContext frame) =>
        from rows in IO.lift(() => Take(lane, sink.Bind.Batch.Value))
        from receipt in Settled(sink, cursor, rows, ports, frame)
        select receipt;

    static IO<Fin<EgressReceipt>> Settled(
        Subscription sink, OutboxCursor cursor, Seq<OpLogEntry> rows, EgressPorts ports, ProjectionContext frame) =>
        from mark in IO.lift(frame.Mark)
        from folded in rows.FoldM(
            (Through: cursor.Sequence, Committed: cursor.Sequence,
                Delivered: 0, Duplicates: 0, Filtered: 0, Held: 0, Dead: 0, Open: true),
            (state, row) => !state.Open
                ? IO.pure(state with { Held = state.Held + 1 })
                // Minting rides a rail, so a row whose grammar the branch owner refuses letters HERE rather than
                // reaching a transport that would accept the malformed value: a refused mint cannot succeed on a
                // later attempt, which is exactly the poison the letter table exists for. `Matches` runs after the
                // mint because every dialect reads admitted ATTRIBUTES, so a filter and a broker see one value.
                : Egress.Envelope(row, sink, ports).Bind(minted => minted.Match(
                    Fail: error => Lettered(row, sink, ports, frame, error)
                        .Bind(IO.liftFin)
                        .Map(committed => state with {
                            Through = committed.Sequence,
                            Committed = committed.Sequence,
                            Dead = state.Dead + 1,
                        }),
                    Succ: envelope => !sink.Matches(envelope).Holds
                        ? IO.pure(state with { Through = row.Sequence, Filtered = state.Filtered + 1 })
                        : sink.Deliver(envelope, row).Bind(ack => ack.Switch(
                            persisted:     p  => IO.pure(state with { Through = row.Sequence, Delivered = state.Delivered + 1, Duplicates = state.Duplicates + (p.Duplicate ? 1 : 0) }),
                            indeterminate: _  => IO.pure(state with { Held = state.Held + 1, Open = false }),
                            refused:       rf => Lettered(row, sink, ports, frame,
                                new EgressFault.SinkRefused(sink.Bind.Key, rf.Detail))
                                .Bind(IO.liftFin)
                                .Map(committed => state with {
                                    Through = committed.Sequence,
                                    Committed = committed.Sequence,
                                    Dead = state.Dead + 1,
                                })))))).As()
        from advance in folded.Through > folded.Committed
            ? ports.Coordinate(new CoordinationOp.OutboxAdvance(sink.Bind.Key, folded.Through), sink.Bind.Held).Map(static result => result.Map(static _ => unit))
            : IO.pure(Fin<Unit>.Succ(unit))
        let receipt = new EgressReceipt(sink.Bind.Key, new EgressWindow.Cursor(cursor.Sequence, folded.Through),
            rows.Count, folded.Delivered, folded.Duplicates, folded.Filtered, folded.Held, folded.Dead,
            frame.Elapsed(mark), frame.Now(), frame.Correlation)
        select advance.Match(Succ: _ => Fin<EgressReceipt>.Succ(receipt), Fail: error => Fin<EgressReceipt>.Fail(error));

    // `Attempts: 1` is the MEASURED first attempt, not a filled slot: this fold is the only site a letter is minted
    // at and the row it letters was offered exactly once. Every later count comes from `Replay`'s own `Attempts + 1`,
    // so the column is monotone from its first write and no arm publishes a count no delivery produced. ONE lettering
    // seat serves the refused mint and the refused delivery, because both are the same terminal verdict over one row
    // and a second minting site would fork the attempt column's monotonicity.
    static IO<Fin<OutboxCursor>> Lettered(
        OpLogEntry row, Subscription sink, EgressPorts ports, ProjectionContext frame, Error fault) =>
        ports.QuarantineAndAdvance(
            new DeadLetterRow(
                row.ContentKey, sink.Bind.Key, row.Sequence,
                Observation(row.ContentKey, sink.Bind.Key, ports, fault), Attempts: 1, frame.Now()),
            sink.Bind.Held);

    static global::Rasm.Contracts.Fault.FaultObservation Observation(
        UInt128 contentKey, SinkKey sink, EgressPorts ports, Error cause) =>
        ports.ObserveFault(new EgressFault.DeadLetter(contentKey, sink, cause));

    // Replay IS the drain fold re-parameterized over the letter set, never a second delivery path. The set is READ
    // here rather than handed in, so no caller can pair a sink with another sink's or another tenant's letters. A
    // `Persisted` retires the letter and a still-refusing row re-letters with `Attempts + 1`: attempts are MONOTONE
    // and `Retire` is the one terminal, so no reset arrow can fabricate a fresh budget for a poison row. A vanished
    // row retires as `Held`, so the conservation fold closes over letters exactly as the drain closes over rows.
    public static IO<Fin<EgressReceipt>> Replay(
        Subscription sink, EgressPorts ports, ProjectionContext frame) =>
        ports.Profile.Admits(Lane)
            ? Replayed(sink, ports, frame)
            : Unrealizable<EgressReceipt>(sink.Bind.Key);

    static IO<Fin<EgressReceipt>> Replayed(Subscription sink, EgressPorts ports, ProjectionContext frame) =>
        from mark in IO.lift(frame.Mark)
        from letters in ports.Letters(sink.Bind.Key, sink.Bind.Batch.Value)
        from folded in letters.FoldM(
            (Delivered: 0, Duplicates: 0, Filtered: 0, Held: 0, Dead: 0),
            (state, letter) =>
                from rows in ports.Feed(ReplayWindow.DurableOps(letter.Sequence - 1, 1))
                let found = rows.Filter(r => r.ContentKey == letter.ContentKey).Head
                from next in found.Match(
                    // Letters whose envelope still refuses to mint re-letter on the SAME monotone attempt column as a
                    // refusing delivery, so a poisoned grammar value and a poisoned transport share one budget and
                    // one terminal rather than one of them looping outside the schedule.
                    Some: row => Egress.Envelope(row, sink, ports).Bind(minted => minted.Match(
                        Fail: error => ports.Reletter(letter with {
                            Fault = Observation(letter.ContentKey, letter.Sink, ports, error),
                            Attempts = letter.Attempts + 1,
                            At = frame.Now(),
                        }).Map(_ => state with { Dead = state.Dead + 1 }),
                        Succ: envelope => !sink.Matches(envelope).Holds
                            ? ports.Retire(letter).Map(_ => state with { Filtered = state.Filtered + 1 })
                            : from ack in sink.Deliver(envelope, row)
                              from settled in ack.Switch(
                                persisted:     p  => ports.Retire(letter).Map(_ => state with { Delivered = state.Delivered + 1, Duplicates = state.Duplicates + (p.Duplicate ? 1 : 0) }),
                                indeterminate: _  => IO.pure(state with { Held = state.Held + 1 }),
                                refused:       rf => ports.Reletter(letter with {
                                    Fault = Observation(letter.ContentKey, letter.Sink, ports,
                                        new EgressFault.SinkRefused(sink.Bind.Key, rf.Detail)),
                                    Attempts = letter.Attempts + 1,
                                    At = frame.Now(),
                                }).Map(_ => state with { Dead = state.Dead + 1 }))
                              select settled)),
                    None: () => ports.Retire(letter).Map(_ => state with { Held = state.Held + 1 }))
                select next).As()
        select Fin<EgressReceipt>.Succ(new EgressReceipt(sink.Bind.Key, new EgressWindow.Replay(), letters.Count,
            folded.Delivered, folded.Duplicates, folded.Filtered, folded.Held, folded.Dead, frame.Elapsed(mark), frame.Now(), frame.Correlation));
}
```

| [INDEX] | [POLICY]      | [VALUE]                                        | [BINDING]                                                        |
| :-----: | :------------ | :--------------------------------------------- | :--------------------------------------------------------------- |
|  [01]   | drain source  | `ReplayWindow.DurableOps` past the sink cursor | one windowed read (ledger); presence never enters                |
|  [02]   | advance law   | contiguous settled prefix                      | terminal quarantine commits its exact advance atomically         |
|  [03]   | replay        | `Letters` loads; the same fold re-delivers     | `Reletter` updates an already-advanced row; `Retire` is terminal |
|  [04]   | wake          | `WaitAsync` on `rasm_outbox` + bounded poll    | NOTIFY is latency; the poll floor owns correctness               |
|  [05]   | payload arrow | `Frame` before the mint                        | fail-closed `ErasingRedactor`; grade, media, and schema as data  |
|  [06]   | filter        | the subscription's own AND-set, post-mint      | a withheld row settles and advances; it is no transport outcome  |
|  [07]   | receipt floor | conservation `ValidityClaim.All` fold          | delivered + filtered + held + dead == drained, once              |

## [03]-[EGRESS_SINK]

- Owner: `Egress.Envelope` the ONE mint of an `OpLogEntry`, composing `Rasm/Domain/event#ENVELOPE_MINT` so the branch owner's `Validate()` funnel runs on every projected row and the projection returns `IO<Fin<CloudEvent>>` — the residence write a body past the row's `dataref` threshold takes is the one effect a mint carries, and it lands BEFORE the envelope publishes the address; `BindingCapability`/`BindingCaps` the transport capability vocabulary and its barred empty corner; `Binding` the transport roster carrying each transport's capability set, attribute prefix, routing member, settings roster, `dataref` policy, and honest degrade as COLUMNS; `ProtocolSettings` the admitted per-subscription slice over that roster; `Subscription` the delivery instance; `DeliveryAck` the one union every provider outcome folds to at its own boundary, so a raw `PubAckResponse`/`DeliveryResult`/`MessageId` never crosses into the pump and the union itself names no provider type; `KafkaAck` the one written-out leg fold, seated as a boundary owner beside the union rather than inside it; `SinkBinding.Watch` the cell every leg consults before folding.
- Entry: `Egress.Envelope(row, sink, ports)` mints on the `IO` rail; `Subscription.Deliver(envelope, row)` resolves the bound leg; `Subscription.Matches(envelope)` answers the filter AND-set; `ProtocolSettings.Admit(binding, settings, key)` is the ONE admission every subscription crosses at composition.
- Cases: closed transport rows over ONE envelope, each settling its own engine's answer rather than a shared convention. `http` enqueues `net.http_post` under an idempotency-key header and folds `net.http_response_result` on the NEXT drain, so a PENDING response resolves without a poller; `nats` reads BOTH `NatsResult.Error` (never reached the stream) and `PubAckResponse.Error` (the stream refused), because folding the result alone reports `Persisted` for a refused ack; `kafka` pins `EnableIdempotence` under an instrumented producer, since the dedup column claims broker-side suppression an unset flag leaves unconfigured, and that one flag then FORCES `acks=all`, `max.in.flight.requests.per.connection=5`, `retries=INT32_MAX`, and `queuing.strategy=fifo` — a producer whose supplied configuration contradicts any of them refuses to instantiate, so the row spells none of them as a settable key and bounds the forced retry through `message.timeout.ms` alone; `rabbitmq` sends `mandatory: true` and reads `PublishException.IsReturn`, because an unroutable message under `false` is discarded while the confirm still ACKS, and under `true` the return and the nack arrive as ONE exception type whose only discriminant is that flag — an unroutable address is terminal and a nack is the broker refusing a routable message, so folding both onto one verdict either letters a re-drivable row or re-drives an address no retry reaches; `pulsar` writes `SequenceId` as the durable op-log position incremented by one, because broker dedup keys on `(producer name, sequence id)` MONOTONICITY against a stored high-water mark and DotPulsar reads a ZERO id as the auto-assign sentinel for a per-producer counter that restarts at `InitialSequenceId` on every construction — a content key written there is unordered by construction and roughly half of every send lands at or beneath the mark, while the auto counter re-lands the whole replayed suffix beneath it after any restart, and BOTH are discarded as duplicates against a returned `MessageId` the fold reads as `Persisted`; only the op-log sequence stays monotone per stream and durable across restarts, and the increment keeps position zero off the sentinel; `redis`, `clickhouse`, and `wirenative` are house legs whose `Spec` column says so.
- Cases: the AMQP row bounds its OWN in-flight window because the client publishes no sender-side credit member — the peer grants credit on its `Flow`, `SetCredit` has no sender counterpart, and the callback send appends to an unbounded internal list the moment credit is absent — so the awaited pair is the only admitted send and the window is a bounded `Channel` under `Wait`, settled in OFFER order exactly as the NATS concurrent futures settle. Row `mqtt` is BRANCH-OWNED because the published package compiles against a retired carrier shape and reaches structured mode alone; it pins `V500`, since every v5 field drops SILENTLY under `V311` — no throw, no reason code — and carries the one UNPREFIXED attribute map, a `ce-` or `ce_` spelling there being a conformance defect a peer silently drops.
- Cases: the ClickHouse row's `Watch` cell is the one standing behind NO event — that driver declares zero connection events, never raises the inherited `StateChange`, and ships no pool type — so `State` echoes only an explicit `Open`/`Close` this fence already made. Instead an ACTIVE `PingAsync` probe on the composition root's own cadence feeds the same cell every event-bearing row writes, so the `Watch` arrow holds ONE shape across the family and only this row's producer differs; its table IS the `Query/datasets#WAREHOUSE_OPLOG` `WarehouseSchema.Table` with its `Columns` roster, and its read side the `Query/residence#RESIDENCE_FAMILY` `Residence.Fleet` row, so writer and reader share one typed vocabulary rather than two independently-authored shapes.
- Auto: refusal SHAPE is a column too, and the one every naive adapter gets wrong — `throw` alone covers redis and clickhouse, MQTT is `value` only since its publish never throws on the wire, Pulsar adds reactive `IState`, and RabbitMQ, NATS, and Kafka add events and callbacks — so the family reads its awaited return AND the row's `Watch` cell rather than assuming one rail every engine shares. Dedup honesty is a COLUMN, not prose: every row states what absorbs a replay — NATS the broker window over `NatsJSPubOpts.MsgId` carrying the content key, which is the SDK's own dedup carriage and the one spelling a durable publish takes, since a hand-written `Nats-Msg-Id` header re-implements the member that owns it and forks the key every peer branch already sets through its own option; Kafka the idempotent producer with consumer-side `(source, id)`; pulsar the `(producer name, sequence id)` window over the op-log position; http and wire-native and amqp and mqtt receiver-side dedup on that same composite; redis its `StreamIdempotentId`; clickhouse its `insert_deduplication_token`.
- Auto: the completed generated `Extensions` value crosses the kernel's descriptor-total contract once; that bridge validates before projection. Generated `sequence` is invariant `D20` unsigned decimal, so lexical order agrees with `uint64` order.
- Auto: content mode is the binding row's own capability set and the subscription's settings selection within it, never a per-leg literal — every header-bearing transport reaches binary so a broker filters on the prefixed attribute names without parsing the body, and each row's prefix (`ce-` HTTP/NATS/RabbitMQ, `ce_` Kafka, `cloudEvents_` AMQP, UNPREFIXED MQTT) is a column rather than a spelling each leg repeats. Serdes-governed Kafka bodies own the `Data` bytes and their schema-id framing beside the `ce_*` headers with zero key collision, because the payload arrow frames them over one registry client BEFORE the envelope — the Avro and JSON-Schema serdes alone, since the durable payload is lane-codec bytes and never an `IMessage<T>` — so envelope codec and body codec never share a `JsonSerializerOptions`.
- Auto: the `http` row's multi-row drain encodes ONE batch body only under a receiving contract that advertises the batch media type, never on the transport's own say-so: `net.http_post` hands back one id, `net._http_response` stores ONE status against it, and the CloudEvents batch binding defines no per-event response element — so a receiver settling per REQUEST answers N envelopes with a single status and the drain reports a merged tally that cannot tell zero redelivery from a wedged retry. `PerRequest` is therefore the pg_net floor and its cursor-advancing drain posts SINGLE-row bodies. `PerEnvelope` admits batching only when the receiver returns each disposition with its exact `(source,id)`; correlation rebuilds offer order from identity after the response and refuses missing, duplicate, or foreign keys, so batch position and intermediary reframing carry no settlement meaning.
- Receipt: per-subscription delivery evidence rides the drain receipt (`#EGRESS_PUMP`); a subscription names its cursor row through `SinkBinding.Key` and its transport through the `Binding` row, never a free string.
- Packages: Rasm.Contracts (`event.Extensions`), Celly.Protovalidate, Rasm (CloudEvents mechanics and interior handling policy), CloudNative.CloudEvents with Kafka and AMQP bindings, Pidgin, NATS.Net, Confluent.Kafka, Confluent.SchemaRegistry, OpenTelemetry.Instrumentation.ConfluentKafka, RabbitMQ.Client, MQTTnet, DotPulsar, StackExchange.Redis, AMQPNetLite.Core, System.Threading.Channels, ClickHouse.Driver, pg_net, BCL inbox.
- Growth: a new delivery target is one `Subscription` value; a new transport one `Binding` row; a new extension changes `event.proto`, while this producer adds only the value it actually supplies.
- Boundary: the envelope is the single cross-consumer, cross-language vocabulary — the AppHost outbox relay and the durable-orchestration dispatch drain the SAME projection as their hop payload, so a per-consumer re-pack is the drift defect. `id` is the OPERATION identity and `subject` the content key, so replay dedup reads `(source, id)` and a broker sequence keys nothing. Row `http` NEVER fire-and-forgets: `net.http_post` enqueues and the response reconciliation is the only advance authority.
- Boundary: a payload past the row's `dataref` threshold externalizes to `Store/blobstore#OBJECT_STORE` and the envelope carries the reference, so no leg holds a multi-megabyte body to encode and no streaming encoder exists beside the owner's one encode. Row `wirenative` reads the AppHost delivery-honesty policy — the database is excluded from the AppHost hop law, sink delivery is not — and the redis row's acknowledged trim keeps the stream bounded by CONSUMPTION rather than a time guess. This family is egress-only: the inbound Kafka consume leg is the `Version/ingress` `CdcIngress` owner where the consumer-side instrumented twins bind, never a binding row here, and its `(source, id)` dedup is the consumer half every dedup-honesty row presumes.

```csharp signature
// --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
using System.Net.Mime;
using CloudNative.CloudEvents;
using Rasm.Domain;                                // the branch envelope owner (Domain/event) + Op, the operation key every fallible kernel threads
// Aliases carry the ONE provider-typed surface on this page rather than a namespace import, because `Error` is
// spelled by both `Confluent.Kafka` and the rail: each reference site scope-qualifies and no ambiguity reaches a reader.
using KafkaError = Confluent.Kafka.Error;
using KafkaStatus = Confluent.Kafka.PersistenceStatus;

namespace Rasm.Persistence.Version;

// --- [TYPES] ----------------------------------------------------------------------------

// Kafka publication mode controls broker visibility only; the PostgreSQL cursor remains outside the transaction.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class KafkaPublishMode {
    public static readonly KafkaPublishMode Awaited = new("awaited");
    public static readonly KafkaPublishMode ReadCommitted = new("read-committed");
}

// Receiving-CONTRACT column deciding whether one request may carry a whole drain window. `PerRequest` is the pg_net
// floor — one enqueued request answers with one stored status, and the CloudEvents batch binding names no per-event
// response element, so a batched body under it fills `Delivered` and `Duplicates` from a single number and publishes
// a duplicate count nothing measured. `PerEnvelope` admits the batch encode because the RECEIVER publishes a
// disposition keyed by each envelope's exact `(source,id)`. Correlation reconstructs the sender's offer order from
// those keys for contiguous-prefix advance; batch position is never identity, and missing, duplicate, or foreign
// dispositions refuse rather than pairing an acknowledgement with the wrong durable row.
public readonly record struct DeliveryDisposition(string Source, string Id, DeliveryAck Ack) {
    public string Key => $"{Source}\0{Id}";
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record WebhookSettle {
    private WebhookSettle() { }

    public sealed record PerRequest : WebhookSettle;
    public sealed record PerEnvelope(Func<string, int, Fin<Seq<DeliveryDisposition>>> Read) : WebhookSettle {
        public Fin<Seq<DeliveryAck>> Correlate(string request, Seq<CloudEvent> offered, Op key) =>
            Read(request, offered.Count).Bind(answered => Correlated(offered, answered, key));
    }

    private static Fin<Seq<DeliveryAck>> Correlated(
        Seq<CloudEvent> offered, Seq<DeliveryDisposition> answered, Op key) => key.Catch(() => {
        Dictionary<string, DeliveryAck> byIdentity = new(StringComparer.Ordinal);
        foreach (DeliveryDisposition disposition in answered) {
            if (!byIdentity.TryAdd(disposition.Key, disposition.Ack)) {
                return Fin.Fail<Seq<DeliveryAck>>(new EgressFault.SettingsRejected(
                    Binding.Http.Key, $"<duplicate-disposition:{disposition.Key}>", Some(key)));
            }
        }

        Seq<DeliveryAck> correlated = Seq<DeliveryAck>();
        foreach (CloudEvent envelope in offered) {
            string identity = $"{envelope.Source}\0{envelope.Id}";
            if (!byIdentity.Remove(identity, out DeliveryAck? ack) || ack is null) {
                return Fin.Fail<Seq<DeliveryAck>>(new EgressFault.SettingsRejected(
                    Binding.Http.Key, $"<absent-disposition:{identity}>", Some(key)));
            }
            correlated = correlated.Add(ack);
        }

        return byIdentity.Count == 0
            ? Fin.Succ(correlated)
            : Fin.Fail<Seq<DeliveryAck>>(new EgressFault.SettingsRejected(
                Binding.Http.Key, $"<foreign-disposition:{string.Join(',', byIdentity.Keys)}>", Some(key)));
    });
}

// Per-binding externalization policy. A threshold fixed estate-wide either strands the smallest transport or
// wastes the largest, so it derives from the transport's OWN negotiated limit. `Dual` gates reference-ALONE
// shipping, since the specification carries no capability negotiation and a peer that cannot resolve a reference
// has no way to say so. Retention belongs to the payload's object-plane admission, never the transport row;
// `Residence` binds once at the composition root as the
// `Store/blobstore#OBJECT_STORE` port, and an unbound port refuses at admission rather than shipping a reference
// nothing resolves.
public readonly record struct DatarefRow(int Threshold, bool Dual);

// Every binding row holds its transport capabilities as ONE set: the content modes it carries, whether it batches,
// and whether the BROKER resolves a routing filter. The empty corner is barred — a binding admitting no content mode
// delivers nothing — which is the law a `FrozenSet<ContentMode>` beside two bool columns could not state and no arm
// could answer.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class BindingCapability : ICapability<BindingCapability> {
    public static readonly BindingCapability Binary = new("binary");
    public static readonly BindingCapability Structured = new("structured");
    public static readonly BindingCapability Batches = new("batches");
    // MQTT resolves on SUBSCRIBE, AMQP on a link-source filter, NATS on a subject wildcard. Kafka and HTTP have no
    // server-side mechanism at all, so their filters run consumer-side and a routing dialect on those rows costs a
    // delivered-then-discarded message rather than nothing.
    public static readonly BindingCapability Pushdown = new("pushdown");
}

public static class BindingCaps {
    public static readonly CapabilityLaw<BindingCapability> Law =
        CapabilityLaw<BindingCapability>.Forbidden(Seq(CapabilitySet<BindingCapability>.None));
    public static CapabilitySet<BindingCapability> Of(params ReadOnlySpan<BindingCapability> held) =>
        CapabilitySet<BindingCapability>.Of(held);
}

// ONE transport roster. Every column is a fact the SPECIFICATION or the engine fixes, so a subscription selects a row
// and supplies values — never a per-sink knob re-deciding what the transport already answers. `Spec` marks the rows a
// CloudEvents protocol binding backs; a false row is a house envelope leg carrying the same envelope over a property
// map of its own, which is an honest column rather than a silent second vocabulary.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class Binding {
    // Prefixes follow each binding and a leg reusing a neighbour's publishes names no conforming peer resolves:
    // HTTP/NATS/RabbitMQ carry `ce-`, Kafka carries `ce_`, AMQP 1.0 carries `cloudEvents_`, and MQTT 5.0 carries no
    // prefix at all.
    public static readonly Binding Http = new("http",
        caps: BindingCaps.Of(BindingCapability.Binary, BindingCapability.Structured, BindingCapability.Batches), prefix: "ce-",
        routesOn: None, required: ["endpoint"], optional: ["method", "headers"],
        dataref: new(Threshold: 8 << 10, Dual: false), spec: true,
        degrade: "one status per request, and one PENDING holds the cursor for a whole drain");

    // `acks` is NOT a settable key here: `EnableIdempotence` forces `acks=all` and librdkafka states "Producer
    // instantation will fail if user-supplied configuration is incompatible", so the only value the key could carry
    // is the one the row already pins and every other value is a boot failure wearing configuration's clothes.
    // `messagetimeoutms` IS settable because it is this leg's whole retry BOUND — see the `[RETRY_OWNER]` column.
    public static readonly Binding Kafka = new("kafka",
        caps: BindingCaps.Of(BindingCapability.Binary, BindingCapability.Structured), prefix: "ce_",
        routesOn: Some(global::Rasm.Contracts.Event.Extensions.PartitionkeyFieldNumber), required: ["topicname"], optional: ["partitionkeyextractor", "clientid", "messagetimeoutms", "mode"],
        dataref: new(Threshold: 1 << 20, Dual: false), spec: true,
        degrade: "`Error.IsRetriable` is internal; transactions never span the cursor");

    // MQTT is the one UNPREFIXED row: v5 User Properties carry the bare attribute names, so a `ce_` or `ce-` spelling
    // here is a conformance defect a peer silently drops rather than a stylistic choice.
    public static readonly Binding Mqtt = new("mqtt",
        caps: BindingCaps.Of(BindingCapability.Binary, BindingCapability.Structured, BindingCapability.Pushdown), prefix: "",
        routesOn: None, required: ["topicname"], optional: ["qos", "retain", "expiry", "userproperties"],
        dataref: new(Threshold: 256 << 10, Dual: true), spec: true,
        degrade: "no key and no origin; every v5 field drops silently under `V311`, which is structured-only");

    public static readonly Binding Amqp = new("amqp",
        caps: BindingCaps.Of(BindingCapability.Binary, BindingCapability.Structured, BindingCapability.Pushdown), prefix: "cloudEvents_",
        routesOn: None, required: ["address"], optional: ["linkname", "sendersettlementmode", "linkproperties", "inflight"],
        dataref: new(Threshold: 512 << 10, Dual: false), spec: true,
        degrade: "no sender credit member at all; this fence bounds its own in-flight");

    // `retryattempts` is settable because it is this leg's second attempt owner: `NatsJSPubOpts.RetryAttempts`
    // defaults to 1 — one attempt total, and only a `NoResponders` refusal re-offers at all — so a row leaving it
    // unstated publishes a schedule nothing on this page declared. The dedup identity is NOT a settable key: it is
    // `NatsJSPubOpts.MsgId` off the content key, an SDK member no deployment overrides.
    public static readonly Binding Nats = new("nats",
        caps: BindingCaps.Of(BindingCapability.Binary, BindingCapability.Structured, BindingCapability.Pushdown), prefix: "ce-",
        routesOn: None, required: ["subject"], optional: ["stream", "retryattempts"],
        dataref: new(Threshold: 1 << 20, Dual: false), spec: true,
        degrade: "no key member, so per-entity order needs one subject per entity");

    // `inflight` is settable for the same reason the AMQP row carries it and NOT because the client publishes no
    // bound: `CreateChannelOptions` initializes its confirms limiter field to a throttling default, but the public
    // constructor's parameter for it defaults to NULL, so the two-argument confirm-enabling call every naive adapter
    // writes disables rate limiting outright and leaves unconfirmed publishes unbounded. The value is PASSED or it
    // does not exist.
    public static readonly Binding RabbitMq = new("rabbitmq",
        caps: BindingCaps.Of(BindingCapability.Binary, BindingCapability.Structured), prefix: "ce-",
        routesOn: None, required: ["exchange", "routingkey"], optional: ["expiration", "inflight"],
        dataref: new(Threshold: 1 << 20, Dual: false), spec: true,
        degrade: "auto-recovery swallows a drop the caller never observes");

    // House legs: the same envelope over a property map the specification never defined, so `Spec` is false and
    // `Prefix` is empty because there is no prefixed attribute contract to honour or to violate.
    public static readonly Binding Pulsar = new("pulsar",
        caps: BindingCaps.Of(BindingCapability.Structured), prefix: "",
        routesOn: Some(global::Rasm.Contracts.Event.Extensions.PartitionkeyFieldNumber), required: ["topic"], optional: ["accessmode"],
        dataref: new(Threshold: 5 << 20, Dual: false), spec: false,
        degrade: "no transactions; a fenced producer surfaces only on `IState`");

    public static readonly Binding Redis = new("redis",
        caps: BindingCaps.Of(BindingCapability.Structured), prefix: "",
        routesOn: None, required: ["stream", "group"], optional: [],
        dataref: new(Threshold: 512 << 10, Dual: false), spec: false,
        degrade: "the consumer group's cursor never governs the outbox cursor");

    public static readonly Binding ClickHouse = new("clickhouse",
        caps: BindingCaps.Of(BindingCapability.Structured, BindingCapability.Batches), prefix: "",
        routesOn: None, required: ["table"], optional: [],
        dataref: new(Threshold: 4 << 20, Dual: false), spec: false,
        degrade: "no events at all; `BeginDbTransaction` throws, the token is the dedup story");

    public static readonly Binding WireNative = new("wirenative",
        caps: BindingCaps.Of(BindingCapability.Structured), prefix: "",
        routesOn: None, required: ["hopkey"], optional: [],
        dataref: new(Threshold: 4 << 20, Dual: false), spec: false,
        degrade: "no broker behind the hop, so an undelivered envelope rests in the letter table");

    private Binding(string key, CapabilitySet<BindingCapability> caps, string prefix,
        Option<int> routesOn, FrozenSet<string> required, FrozenSet<string> optional,
        DatarefRow dataref, bool spec, string degrade) : this(key) =>
        (Caps, Prefix, RoutesOn, Required, Optional, Dataref, Spec, Degrade) =
        (caps, prefix, routesOn, required, optional, dataref, spec, degrade);

    public CapabilitySet<BindingCapability> Caps { get; }
    public string Prefix { get; }
    public Option<int> RoutesOn { get; }
    public FrozenSet<string> Required { get; }
    public FrozenSet<string> Optional { get; }
    public DatarefRow Dataref { get; }
    public bool Spec { get; }
    public string Degrade { get; }

    // Every leg carries the RICHEST mode its row admits, so the `ContentMode` a binding hands its formatter derives
    // from the set rather than from a parallel roster a caller could disagree with.
    public ContentMode Mode => Caps.Admits(BindingCapability.Binary) ? ContentMode.Binary : ContentMode.Structured;

    // Derived membership, never a second roster: a caller asking which transports resolve a filter at the broker
    // reads the rows' own set, so a new row joins both answers with no edit here.
    public static readonly Lazy<FrozenSet<Binding>> BrokerFiltered =
        new(static () => Items.Where(static row => row.Caps.Admits(BindingCapability.Pushdown)).ToFrozenSet());

    public static readonly Fin<Unit> Lawful =
        toSeq(Items).Traverse(static row => BindingCaps.Law.Admit(row.Caps)).As().Map(static _ => unit);
}

// --- [MODELS] ---------------------------------------------------------------------------

// Specification `protocolsettings` slices admit ONCE against the binding row's own roster. Unknown keys FAIL CLOSED
// and missing required keys refuse, so a settings map is proven readable before a subscription exists — a key
// accepted and silently ignored is worse than a rejected one, since it publishes a governance value no leg ever
// consumes. Values stay text because the wire carries text; each leg parses its own.
public sealed record ProtocolSettings {
    private ProtocolSettings(Map<string, string> values) => Values = values;

    public Map<string, string> Values { get; }

    public static Fin<ProtocolSettings> Admit(Binding binding, Map<string, string> settings, Op key) =>
        toSeq(binding.Required).Filter(name => settings.Find(name).IsNone) is { IsEmpty: false } absent
            ? Fin.Fail<ProtocolSettings>(new EgressFault.SettingsRejected(binding.Key, $"<absent:{string.Join(',', absent)}>", Some(key)))
            : toSeq(settings.Keys).Filter(name => !binding.Required.Contains(name) && !binding.Optional.Contains(name)) is { IsEmpty: false } unknown
                ? Fin.Fail<ProtocolSettings>(new EgressFault.SettingsRejected(binding.Key, $"<unknown:{string.Join(',', unknown)}>", Some(key)))
                : Fin.Succ(new ProtocolSettings(settings));

    // Required keys read TOTAL by construction — admission already proved presence, so the interior never re-tests
    // and never carries a null branch for a value the boundary settled.
    public string this[string name] => Values[name];

    public Option<string> Optional(string name) => Values.Find(name);
}

// ONE ack family every provider outcome folds to at its own boundary — a raw PubAckResponse / DeliveryResult /
// MessageId / request_status never crosses into the pump. Only Persisted advances.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record DeliveryAck {
    private DeliveryAck() { }
    public sealed record Persisted(bool Duplicate) : DeliveryAck;
    public sealed record Indeterminate(string Detail) : DeliveryAck;
    public sealed record Refused(string Detail) : DeliveryAck;
}

// --- [BOUNDARIES] -----------------------------------------------------------------------

// `KafkaAck` owns the Kafka leg's boundary fold, seated apart from the family it folds INTO. `DeliveryAck` carries
// one shared vocabulary every transport reaches, so a provider enum parameterizing one of its own statics seats
// one engine's answer inside the type the other nine share — exactly the fork the family exists to prevent. Every
// other leg folds at its own composition-root `Leg`; this one writes out because its three-valued status is the
// single case where a naive adapter reads a returned value as delivery.
public static class KafkaAck {
    // NotPersisted AND PossiblyPersisted are RETRIABLE: a definitively-not-persisted row re-drives under the held
    // cursor with zero duplication risk; Refused is reserved for the caught ProduceException the leg converts —
    // mapping a retriable outcome to Refused quarantines a safe row.
    public static DeliveryAck FromResult(KafkaStatus status, string detail) => status switch {
        KafkaStatus.Persisted => new DeliveryAck.Persisted(Duplicate: false),
        _                     => new DeliveryAck.Indeterminate(detail),
    };

    // Thrown-side twin of the same fold, taking the `Error` off the raised `ProduceException`'s own base rather than
    // its `DeliveryResult` — that property is typed `DeliveryResult<,>` and NOT `DeliveryReport<,>`, so it carries no
    // `Error` at all and its `Key`/`Value`/`Timestamp`/`Headers` proxies raise on the null `Message` a refused
    // produce leaves behind. `IsRetriable` is INTERNAL on this client, so `IsFatal` is the one public discriminator
    // a leg can read beside `Code`, `IsLocalError`, and `IsBrokerError`: a non-fatal produce fault (leader election,
    // transient broker refusal) re-drives under the held cursor, and only a fatal one earns the letter table.
    // Collapsing every caught ProduceException onto Refused quarantines rows the very next attempt would have taken.
    public static DeliveryAck FromError(KafkaError error) =>
        error.IsFatal ? new DeliveryAck.Refused(error.Reason) : new DeliveryAck.Indeterminate(error.Reason);
}

// `SinkBinding` is the shared subscription row data: the cursor-row key, the drain batch width, the held fencing
// token the cursor advance validates, the bound delivery leg, and the out-of-band fault reader — the composition root
// fills `Leg` from the provider client (the blobstore `GrantMinter` idiom), so provider SDK types never enter this
// fence.
//
// `Watch` closes the result-shaped rail trap at the FAMILY owner rather than per provider. Six transports report a
// delivery failure somewhere the awaited return cannot reach: Pulsar publishes `ProducerState.Faulted`/`Fenced` on a
// reactive `IState` await, RabbitMQ delivers an unroutable `BasicReturnEventArgs` and every connection fault on an
// event, Kafka hands `Error` to `SetErrorHandler`, NATS raises
// `MessageDropped`/`SlowConsumerDetected`/`ReconnectFailed` as events, MQTT carries its disconnect cause as a FIELD
// on `MqttClientDisconnectedEventArgs`, and every AMQP object — link and connection alike — raises
// `AmqpObject.Closed` (or `AddClosedCallback`) carrying its `Error`, so a link detached by the peer reports through
// this cell rather than through a send that already returned. Each provider's own surface subscribes into one cell
// at the composition root and this arrow reads it, so a leg folding only what its await returned can no longer report
// `Persisted` for a row the transport already told someone else it lost. ClickHouse is the lone row with NO event to
// subscribe: its cell is fed by a cadence `PingAsync` probe instead, which is why this arrow stays a plain cell read
// and never grows a per-provider subscription shape.
public sealed record SinkBinding(
    SinkKey Key,
    Dimension Batch,
    Option<LeaseToken> Held,
    Func<Subscription, CloudEvent, OpLogEntry, IO<DeliveryAck>> Leg,
    Func<Option<string>> Watch);

// Delivery targets are VALUES: one binding row, its admitted settings, its filter AND-set, and its cursor row. Rows
// that were union cases are now values a deployment authors, so a second subscription over one transport (a second
// topic, a narrower filter, a different tenant) costs one row in a table rather than a case in a compiler-held family
// — which is exactly what a subscription resource is for.
public sealed record Subscription(SinkBinding Bind, Binding Binding, ProtocolSettings Settings, Seq<FilterDialect> Filters) {
    // Watch reads the row's out-of-band cell AFTER the leg settles and downgrades a Persisted alone: an acknowledged
    // send standing beside a pending transport fault is exactly the ambiguous case, so the cursor HOLDS and the row
    // re-drives into the binding's own dedup rather than advancing past a row the transport reported lost on a
    // surface the await never saw. Indeterminate and Refused already carry their own detail and take no second one; a
    // clean cell is the common path and costs one Option read.
    public IO<DeliveryAck> Deliver(CloudEvent envelope, OpLogEntry row) =>
        Bind.Leg(this, envelope, row).Map(ack => (ack, Bind.Watch()) switch {
            (DeliveryAck.Persisted, { IsSome: true, Case: string pending }) => new DeliveryAck.Indeterminate(pending),
            _                                                              => ack,
        });

    // `filters` is an AND-set: any false expression withholds delivery, and an EMPTY set delivers, because the
    // identity of conjunction is true and an unfiltered subscription is the common case. Evaluation is TOTAL — every
    // dialect answers a verdict and an accumulated fault list — so a CESQL runtime error withholds the event without
    // darkening the subscription, and the faults ride the drain's own observe point.
    public MatchVerdict Matches(CloudEvent envelope) =>
        Filters.Fold(MatchVerdict.Pass, (held, dialect) => held.And(dialect.Evaluate(envelope)));
}

// --- [OPERATIONS] -----------------------------------------------------------------------

public static class EgressEventExtensions {
    // One generated-message bridge owns declaration, projection, inverse admission, field-kind correspondence,
    // and descriptor validation. This package contributes VALUES only.
    public static readonly EventExtensionContract<global::Rasm.Contracts.Event.Extensions> Contract = new(
        global::Rasm.Contracts.Event.Extensions.Parser,
        global::Rasm.Contracts.Event.Extensions.Descriptor,
        new global::Celly.Protovalidate.Validator([
            global::Rasm.Contracts.Event.EventReflection.Descriptor,
        ]));

    public static Fin<global::Rasm.Contracts.Event.Extensions> Of(
        OpLogEntry row, Subscription sink, PayloadFrame framed, TraceCarrier trace, Op key) =>
        row.Sequence < 0
            ? Fin.Fail<global::Rasm.Contracts.Event.Extensions>(
                key.InvalidInput(nameof(OpLogEntry.Sequence)))
            : key.Catch(() => Fin.Succ(Built(row, sink, framed, trace)));

    static global::Rasm.Contracts.Event.Extensions Built(
        OpLogEntry row, Subscription sink, PayloadFrame framed, TraceCarrier trace) {
        global::Rasm.Contracts.Event.Extensions message = framed.Extensions.Clone();
        message.Sequence = checked((ulong)row.Sequence).ToString("D20", CultureInfo.InvariantCulture);
        message.Dataclassification = framed.Grade.Key;
        // The durable entry stamp is the event-recording instant this projection can replay unchanged. A drain-time
        // clock would re-author one `(source,id)` event on every retry; a contributed producer stamp remains intact.
        message.Recordedtime ??= global::Google.Protobuf.WellKnownTypes.Timestamp.FromDateTimeOffset(
            row.Physical.ToDateTimeOffset());
        Optional(trace.TraceParent).Iter(value => message.Traceparent = value);
        Optional(trace.TraceState).Filter(static value => value.Length > 0).Iter(value => message.Tracestate = value);
        trace.Baggage.Iter(value => message.Baggage = value.Value);
        sink.Binding.RoutesOn.Iter(_ => message.Partitionkey = row.EntityKey);
        framed.Residence.Iter(reference => message.Dataref = reference.ToString());
        return message;
    }

}

public static class Egress {
    // Every `rasm.*` metric name and envelope `type` shares this capability subject and every envelope `type` this
    // folder emits shares, so a board and a subscription join ONE vocabulary and a rename moves both at once.
    public const string Domain = "persistence";

    // `Envelope` is the ONE mint: it composes the branch owner, so `Validate()` runs on every projected row and a
    // malformed grammar value refuses HERE instead of reaching a transport that would take it. `id` carries the
    // OPERATION identity the dot already owns — a content digest there makes two peers stamping identical bytes into
    // one event and drops the second — while the content key rides `subject` under the owner's ONE `ContentHash`
    // spelling. `datacontenttype` and `dataschema` are ROW DATA off the payload arrow that framed the body;
    // `dataschema` is an optional absolute URI identifying `data`, while registry subject/version stays serdes
    // configuration. A registry-framed Avro or JSON-Schema body therefore announces what it is rather than an
    // unconditional octet-stream every consumer then decodes by convention. The trace stamped is the
    // CREATION-time carrier rendered by the propagator that owns the format; the persisted trace pair never
    // fabricates absent baggage, while a carrier that captured an admitted baggage value projects it unchanged.
    // The current hop rides each binding's own headers.
    public static IO<Fin<CloudEvent>> Envelope(OpLogEntry row, Subscription sink, EgressPorts ports) =>
        Resided(row, sink, ports).Map(framed => Minted(row, sink, ports, framed));

    // Externalization is an EFFECT and it lands before the envelope exists, because a `dataref` naming bytes no store
    // holds is a reference the receiver resolves to nothing. A body inside the row's own threshold never reaches the
    // port at all, so the common path stays one pure projection over the framed payload and the residence write
    // happens exactly where the transport's negotiated limit forces it.
    static IO<PayloadFrame> Resided(OpLogEntry row, Subscription sink, EgressPorts ports) =>
        ports.Frame(row) switch {
            var framed when framed.Body.Length <= sink.Binding.Dataref.Threshold => IO.pure(framed),
            var framed => ports.Reside(framed).Map(reference => framed with { Residence = Some(reference) }),
        };

    static Fin<CloudEvent> Minted(OpLogEntry row, Subscription sink, EgressPorts ports, PayloadFrame framed) {
        Op key = Op.Of(name: nameof(Egress));
        EventType type = EventType.Of(
            Domain, subject: row.Family.Key, fact: row.Kind.Fact, major: framed.EventMajor);
        EventSource source = EventSource.Of(domain: Domain, capability: "oplog");
        return
            from id in EventId.Of(value: row.Id.Wire, key: key)
            from extensions in EgressEventExtensions.Of(
                row, sink, framed, row.Trace.Continue().Map(ports.Carrier).IfNone(default(TraceCarrier)), key)
            from envelope in RasmEventEnvelope.Mint(
                new RasmEventMint<global::Rasm.Contracts.Event.Extensions>(
                    Type: type,
                    Source: source,
                    Id: id,
                    Subject: Some(row.ContentKey),
                    Time: row.Physical,
                    DataSchema: framed.DataSchema,
                    DataContentType: Some(framed.ContentType),
                    Data: Carried(framed, sink),
                    Extensions: extensions),
                EgressEventExtensions.Contract,
                key)
            select envelope;
    }

    // Payloads SHIP only where the reference alone cannot serve: `Dual` names a peer holding no residence credential,
    // so the reference rides BESIDE the body it describes; every other externalized row replaces the body with it,
    // which is the whole reason a threshold the transport's own limit fixed exists. Shipping both on every row would
    // make the threshold a decoration that grows the message it was written to shrink.
    static object? Carried(PayloadFrame framed, Subscription sink) =>
        framed.Residence.IsNone || sink.Binding.Dataref.Dual ? framed.Body : null;

    // Each BINDING owns its `dataref` decision, because the threshold is that transport's own negotiated limit: a
    // body the row cannot carry crosses the residence port and the envelope publishes the URI-reference that port
    // answered, which is the location a receiver resolves. `subject` remains the durable row's content identity and
    // never aliases that location. Deriving a reference by re-hashing the body publishes no address at all.
}
```

Selection descriptor — the sentence a row is chosen on, the member a message enters through, the mechanism realizing the closed `none | single | multi` tenancy axis, and who ends a message's life. `[DEGRADE]` is the `Binding` row's own column and publishes there alone, so no table restates what the roster already answers.

| [INDEX] | [BINDING]    | [FITS]                          | [ADMIT]                 | [TENANCY]                  | [LIFETIME]                    |
| :-----: | :----------- | :------------------------------ | :---------------------- | :------------------------- | :---------------------------- |
|  [01]   | `http`       | one HTTP consumer, no broker    | `net.http_post` enqueue | per-tenant target `Uri`    | letter table; no server hold  |
|  [02]   | `nats`       | low-latency dedup-window fan    | `TryPublishAsync`       | account or subject prefix  | `StreamConfig` age/msgs/bytes |
|  [03]   | `kafka`      | high-volume partition log       | `ProduceAsync`          | topic prefix under ACL     | broker topic retention        |
|  [04]   | `rabbitmq`   | routed work queue with confirms | `BasicPublishAsync`     | NATIVE vhost               | `Expiration` + queue TTL      |
|  [05]   | `amqp`       | header-routed `AMQP 1.0` peer   | `SenderLink.SendAsync`  | address prefix             | broker-side, no member        |
|  [06]   | `pulsar`     | geo-replicated tiered log       | `ISend.Send`            | NATIVE `tenant/namespace`  | namespace retention           |
|  [07]   | `wirenative` | in-estate gRPC peer             | `OutboundHop` pipeline  | `TenantContext` on the hop | hop deadline; no persistence  |
|  [08]   | `redis`      | consumer-group work stream      | `StreamAdd`             | key prefix                 | `StreamTrimMode.Acknowledged` |
|  [09]   | `clickhouse` | billion-row analytics ingest    | `InsertBinaryAsync`     | tenant-led sort key        | table TTL                     |
|  [10]   | `mqtt`       | constrained sensor/edge peer    | `PublishAsync`          | topic prefix               | `WithMessageExpiryInterval`   |

Guarantee coordinates every engine DECIDES for itself, and the differences ARE the point: one value repeating across engines that genuinely differ is a row that stopped reading its engine.

| [INDEX] | [BINDING]    | [DELIVER]                          | [ORDER]                      | [SETTLE]                             |
| :-----: | :----------- | :--------------------------------- | :--------------------------- | :----------------------------------- |
|  [01]   | `http`       | at-least-once                      | none                         | reconciled `request_status=SUCCESS`  |
|  [02]   | `nats`       | at-least-once + dedup window       | subject; NO key member       | `PubAckResponse`, two error surfaces |
|  [03]   | `kafka`      | at-least-once, idempotent producer | partition by `Message.Key`   | 3-valued `PersistenceStatus`         |
|  [04]   | `rabbitmq`   | at-least-once, publisher confirms  | queue by routing key; NO key | confirm completion; nack THROWS      |
|  [05]   | `amqp`       | at-least-once                      | link/address; NO key member  | awaited `SendAsync`; refusal THROWS  |
|  [06]   | `pulsar`     | at-least-once + fenced leader      | partition + `OrderingKey`    | `MessageId` alone; NO status enum    |
|  [07]   | `wirenative` | exactly-once-effective             | none                         | `OutboundHop` honesty verdict        |
|  [08]   | `redis`      | at-least-once                      | stream                       | returned stream id                   |
|  [09]   | `clickhouse` | at-least-once                      | none; insert order is no key | awaited insert completion            |
|  [10]   | `mqtt`       | QoS-1 at-least-once                | topic; NO key member         | PUBACK reason code, never a throw    |

Recovery coordinates — where a re-drive resumes, what bounds in-flight work, the SHAPE a refusal arrives in (the rail trap the `SinkBinding.Watch` cell closes), and which owner re-offers an attempt BENEATH the cursor.

`[RETRY_OWNER]` earns a column because leaving a client's own attempt count unnamed grants no single owner — it grants two, the second reading a package default no page declared. `[BOUND]` names a PRODUCER-side member alone, since a consume-side prefetch or pause verb standing there bounds a leg that never consumes.

| [INDEX] | [BINDING]    | [REPLAY]                     | [BOUND]                      | [REFUSE]           | [RETRY_OWNER]       |
| :-----: | :----------- | :--------------------------- | :--------------------------- | :----------------- | :------------------ |
|  [01]   | `http`       | none; receiver `(source,id)` | pg_net queue + window        | value              | held cursor         |
|  [02]   | `nats`       | `DeliverPolicy`/`GetDirect`  | `MaxAckPending` + channel    | value/throw/event  | `RetryAttempts` = 1 |
|  [03]   | `kafka`      | `Seek`/`OffsetsForTimes`     | `QueueBufferingMax*`         | throw/value/report | `MessageTimeoutMs`  |
|  [04]   | `rabbitmq`   | none — queue head only       | passed limiter, else UNBOUND | throw + event      | held cursor         |
|  [05]   | `amqp`       | none; receiver `(source,id)` | the `inflight` window        | throw + event      | held cursor         |
|  [06]   | `pulsar`     | `MessageId` cursorless read  | `MaxPendingMessages`         | throw + state      | `ExceptionHandler`  |
|  [07]   | `wirenative` | receiver `(source,id)`       | hop admission row            | `RpcException`     | `OutboundHop`       |
|  [08]   | `redis`      | `StreamIdempotentId`         | trim by acknowledgement      | throw              | held cursor         |
|  [09]   | `clickhouse` | `insert_deduplication_token` | pooled insert                | throw              | held cursor         |
|  [10]   | `mqtt`       | session state; `(source,id)` | NONE — the caller bounds it  | value only         | held cursor         |

## [04]-[SUBSCRIPTION_FILTER]

- Owner: `FilterDialect` the closed seven-dialect predicate family a `Subscription` carries as an AND-set, answering the Element seam `Query/predicate#PREDICATE_ALGEBRA` `MatchVerdict` — a delivery bit beside the faults its evaluation accumulated, whose negation is fail-closed; `Cesql` the table-driven expression owner over the `sql` dialect, holding the one built-once grammar, the three-type value family, the function table, the implicit-cast matrix, and the accumulating evaluator; `[FaultCase]` the fault roster realizing the kernel `[FaultCase]` floor over the `Cesql` row, and `CesqlFault` the seven specification error types seating its rows on the 8530 band.
- Cases: `exact` case-sensitive equality, `prefix`/`suffix` the two affix tests, `all`/`any` the recursive conjunction and disjunction, `not` the recursive negation, `sql` a CESQL expression. Specification makes `sql` OPTIONAL and this fabric makes it mandatory, because a subscription that can express only attribute affixes pushes every real routing decision back into a consumer that must decode the payload to make it.
- Entry: `FilterDialect.Evaluate(envelope)` answers a seam `MatchVerdict`; `Cesql.Compile(text, key)` parses ONCE at subscription admission and rails an unparseable expression as `EgressFault.SettingsRejected` — never a delivery; `CesqlExpression.Evaluate(envelope)` runs the compiled tree per event.
- Auto: evaluation is TOTAL — every operator, function, and cast answers a value and an accumulated fault list, so a runtime error withholds one event rather than darkening a subscription, and a subscription whose expression names a missing attribute keeps routing every event whose attribute IS present. Identity of the AND-set is delivery, so a subscription carrying no filter delivers; `not` inverts the bit and PRESERVES the faults, because a fault under a negation is still a fault the operator did not observe.
- Auto: `Integer` is 32-bit and the proof is `ABS(-2147483648)` — the negation of the minimum has no 32-bit representation, so the evaluator answers a `MathError` beside a defined value where an unchecked `Math.Abs` throws under this branch's `CheckForOverflowUnderflow`; every arithmetic arm reads the same guard, so overflow is a fault row rather than an escaping exception.
- Auto: the grammar is a PRECEDENCE TABLE folded through `ExpressionParser.Build`, never a recursive-descent ladder over mutable state — one ordered row set per precedence level, `Rec` at the one self-reference the parenthesised sub-expression needs, `Try` at every alternation whose branches share a prefix, and `Labelled` on every terminal so a refusal reports the grammar's own vocabulary rather than a character class.
- Law: pushdown is the `Binding` row's own `Pushdown` column and never a dialect property — MQTT resolves a topic filter at the broker on SUBSCRIBE, AMQP through a link-source filter, NATS through a subject wildcard, while Kafka has no server-side filtering and HTTP no native mechanism, so an `exact` or `prefix` dialect on those two rows costs a delivered-then-discarded message. `sql` is consumer-side on every row, since no broker in the roster evaluates it.
- Law: a compiled expression is a VALUE built once and held for the subscription's life — parsers are immutable values and a grammar constructed per evaluation rebuilds the whole expression graph on every event, which is the difference between a per-event allocation and none.
- Receipt: accumulated faults ride the drain's own `rasm.persistence.egress.delivered` observe point beside the filtered count (`#EGRESS_PUMP`), so an expression quietly erroring on every event is visible as a rate rather than as silence.
- Packages: Pidgin, Rasm (`Fault`), CloudNative.CloudEvents (`CloudEvent.GetAttribute`, `CloudEventAttribute.Format`), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Growth: a new dialect is one `FilterDialect` case and one `Evaluate` arm; a new CESQL function is one `CesqlFunction` row carrying its arity and its total body, and the parser, the cast matrix, and the evaluator read it untouched; a new operator is one precedence-table row.
- Boundary: filters decide DELIVERY and never mutate an envelope, so an expression is a pure read over admitted attributes; the attribute vocabulary an expression may name is the branch owner's declared roster, so an unrostered name answers `MissingAttributeError` rather than reaching an untyped string a producer happened to set; subscription persistence, the management API, and the `protocolsettings` roster seat at `#EGRESS_SINK`.

| [INDEX] | [DIALECT] | [SHAPE]                                    | [PUSHDOWN]                                   |
| :-----: | :-------- | :----------------------------------------- | :------------------------------------------- |
|  [01]   | `exact`   | attribute → value, case-sensitive equality | broker rows, on the routing attribute        |
|  [02]   | `prefix`  | attribute → value `startsWith`             | broker rows where the attribute IS the route |
|  [03]   | `suffix`  | attribute → value `endsWith`               | consumer-side on every row                   |
|  [04]   | `all`     | recursive conjunction over nested dialects | only where every child pushes down           |
|  [05]   | `any`     | recursive disjunction over nested dialects | only where every child pushes down           |
|  [06]   | `not`     | recursive negation                         | consumer-side on every row                   |
|  [07]   | `sql`     | a compiled CESQL expression                | consumer-side always                         |

```csharp signature
// --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
using CloudNative.CloudEvents;
using Pidgin;
using Pidgin.Expression;
using Rasm.Domain;
using static Pidgin.Parser;
using static Pidgin.Parser<char>;

namespace Rasm.Persistence.Version;

// --- [TYPES] ----------------------------------------------------------------------------

// Specification value spaces close at three. Ad-hoc union rather than three wrapper records: the slot IS the type
// tag the cast matrix reads, and `Integer` is 32-bit by declaration — the one fact `ABS(-2147483648)` proves, since a
// 64-bit reading would answer that call cleanly and never reach the `MathError` arm. `ConversionFromValue` keeps its
// generated implicit default, which is what lets every operator body, function row, and literal arm hand its own
// `string`, `int`, or `bool` straight into the carrier — suppressing it forces a wrapper spelling at each of those
// sites for no discriminant the slot does not already carry.
[Union<string, int, bool>(T1Name = "Text", T2Name = "Number", T3Name = "Flag")]
public readonly partial struct CesqlValue;

// Evaluation is TOTAL, so the answer is a value AND its accumulated faults — never a rail choosing one. A
// `Validation` here would force the evaluator to abandon a value the specification requires it to produce, and a bare
// value would discard the diagnostics an operator raised on the way to it.
public readonly record struct CesqlResult(CesqlValue Value, Seq<CesqlFault> Faults) {
    public static CesqlResult Of(CesqlValue value) => new(value, Seq<CesqlFault>());

    public CesqlResult Fault(CesqlFault fault) => this with { Faults = Faults.Add(fault) };

    // Faults union across operands, so a binary node reports both sides' diagnostics rather than the first it
    // happened to evaluate — the accumulation the specification's own error list describes.
    public CesqlResult Join(CesqlResult other, CesqlValue value) => new(value, Faults + other.Faults);
}

// Delivery answers through the Element seam `Query/predicate#PREDICATE_ALGEBRA` `MatchVerdict(Holds, Faults)`, whose
// `Negate` is FAIL-CLOSED — a faulted arm stays non-matching through any surrounding `not`, where the local shape
// this page carried flipped the bit regardless and turned a malformed CESQL expression into a delivery. NAMED
// LOSS: the fault element type widens from `CesqlFault` to the seam's `Error`; WITNESS: every case derives the kernel
// `Fault` floor, so a subscriber reads `IsType<CesqlFault.CastError>()` and `HasCode` directly off the error.

// Seven dialects close as one family: `all`/`any`/`not` NEST the root, so a nested filter tree is a shape
// property the recursion owns and every consumer's dispatch stays total; `sql` carries a COMPILED expression, so the grammar
// crossed its admission before the subscription existed and no delivery ever parses text.
[Union]
public abstract partial record FilterDialect {
    private FilterDialect() { }

    public sealed record Exact(string Attribute, string Value) : FilterDialect;
    public sealed record Prefix(string Attribute, string Value) : FilterDialect;
    public sealed record Suffix(string Attribute, string Value) : FilterDialect;
    public sealed record All(Seq<FilterDialect> Children) : FilterDialect;
    public sealed record Any(Seq<FilterDialect> Children) : FilterDialect;
    public sealed record Not(FilterDialect Child) : FilterDialect;
    public sealed record Sql(CesqlExpression Expression) : FilterDialect;

    // Attribute reads render through the attribute's OWN `Format`, so the three affix dialects compare the exact
    // text the wire carries and a `Timestamp` or `Uri` attribute matches the spelling a peer sees rather than a CLR
    // `ToString()` this fence chose. An absent attribute answers false WITHOUT a fault — the specification's affix
    // dialects are membership tests, and a missing attribute is a legitimate non-match rather than an error, which is
    // precisely the distinction `sql`'s `MissingAttributeError` draws.
    public MatchVerdict Evaluate(CloudEvent envelope) => Switch(
        state: envelope,
        exact:  static (event_, node) => Held(event_, node.Attribute, held => string.Equals(held, node.Value, StringComparison.Ordinal)),
        prefix: static (event_, node) => Held(event_, node.Attribute, held => held.StartsWith(node.Value, StringComparison.Ordinal)),
        suffix: static (event_, node) => Held(event_, node.Attribute, held => held.EndsWith(node.Value, StringComparison.Ordinal)),
        // `All` folds from Pass and `Any` from its own false identity, so an empty child set answers the operator's
        // identity rather than a fabricated verdict.
        all:    static (event_, node) => node.Children.Fold(MatchVerdict.Pass, (held, child) => held.And(child.Evaluate(event_))),
        any:    static (event_, node) => node.Children.Fold(MatchVerdict.Of(false), (held, child) => held.Or(child.Evaluate(event_))),
        not:    static (event_, node) => node.Child.Evaluate(event_).Negate(),
        sql:    static (event_, node) => Answered(node.Expression.Evaluate(event_)));

    // Every compiled expression's own total answer IS the verdict: its value casts to the delivery bit and its
    // accumulated faults ride forward unchanged, so an expression erroring on every event stays visible as a rate.
    static MatchVerdict Answered(CesqlResult answered) => new(CesqlCast.Flag(answered.Value), answered.Faults.Map(static fault => (Error)fault));

    static MatchVerdict Held(CloudEvent envelope, string name, Func<string, bool> test) =>
        Optional(envelope.GetAttribute(name)).Bind(attribute => Optional(envelope[attribute]).Map(value => attribute.Format(value)))
            .Match(Some: held => MatchVerdict.Of(test(held)),
                   None: () => MatchVerdict.Of(false));
}

// --- [ERRORS] ---------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CesqlFault : Fault {
    private static readonly FaultBand FamilyBand = FaultBand.Cesql;
    private CesqlFault() { }

    // Only one case reaches a RAIL, carrying the admitting operation's key, exactly as the kernel's own value faults
    // do; the six accumulated cases ride an evaluation with no operation context to name.
    [FaultCase(0)]
    public sealed partial record ParseError(string Detail, Option<Op> Key = default) : CesqlFault();
    [FaultCase(1)]
    public sealed partial record MathError(string Operator, string Detail) : CesqlFault();
    [FaultCase(2)]
    public sealed partial record CastError(string From, string To) : CesqlFault();
    [FaultCase(3)]
    public sealed partial record MissingAttributeError(string Attribute) : CesqlFault();
    [FaultCase(4)]
    public sealed partial record MissingFunctionError(string Function) : CesqlFault();
    [FaultCase(5)]
    public sealed partial record FunctionEvaluationError(string Function, string Detail) : CesqlFault();
    [FaultCase(6)]
    public sealed partial record GenericError(string Detail) : CesqlFault();

    public override string Message => Switch(
        parseError:              static c => $"<cesql-parse>:{c.Detail}",
        mathError:               static c => $"<cesql-math:{c.Operator}>:{c.Detail}",
        castError:               static c => $"<cesql-cast:{c.From}->{c.To}>",
        missingAttributeError:   static c => $"<cesql-attribute:{c.Attribute}>",
        missingFunctionError:    static c => $"<cesql-function:{c.Function}>",
        functionEvaluationError: static c => $"<cesql-eval:{c.Function}>:{c.Detail}",
        genericError:            static c => $"<cesql-generic>:{c.Detail}");
}

// --- [TABLES] ---------------------------------------------------------------------------
// Nine specification functions seat as ROWS, each carrying its arity and its TOTAL body — a body answering a value
// and a fault rather than raising. One table serves the parser (which names are callable), the evaluator (what each
// does), and the diagnostic (what an unknown name reports), so a tenth function is one row and no dispatch site
// moves. `Abs` is the row where the 32-bit width becomes observable.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class CesqlFunction {
    public static readonly CesqlFunction Length = new("LENGTH", arity: 1, static args => CesqlResult.Of(CesqlCast.Text(args[0]).Length));
    public static readonly CesqlFunction Concat = new("CONCAT", arity: -1, static args => CesqlResult.Of(string.Concat(args.Map(CesqlCast.Text))));
    public static readonly CesqlFunction Lower = new("LOWER", arity: 1, static args => CesqlResult.Of(CesqlCast.Text(args[0]).ToLowerInvariant()));
    public static readonly CesqlFunction Upper = new("UPPER", arity: 1, static args => CesqlResult.Of(CesqlCast.Text(args[0]).ToUpperInvariant()));
    public static readonly CesqlFunction Trim = new("TRIM", arity: 1, static args => CesqlResult.Of(CesqlCast.Text(args[0]).Trim()));
    public static readonly CesqlFunction Left = new("LEFT", arity: 2, static args => Slice(args, fromStart: true));
    public static readonly CesqlFunction Right = new("RIGHT", arity: 2, static args => Slice(args, fromStart: false));
    public static readonly CesqlFunction Substring = new("SUBSTRING", arity: 3, static args => Substr(args));

    // Width proves out here: negating `int.MinValue` has no 32-bit representation, so this row answers the minimum
    // beside a `MathError` where an unguarded `Math.Abs` throws under this branch's checked arithmetic — the one call
    // that distinguishes a conforming 32-bit evaluator from a 64-bit one that silently succeeds.
    public static readonly CesqlFunction Abs = new("ABS", arity: 1, static args =>
        CesqlCast.Number(args[0]) is int.MinValue and var floor
            ? CesqlResult.Of(floor).Fault(new CesqlFault.MathError("ABS", "<int32-overflow>"))
            : CesqlResult.Of(Math.Abs(CesqlCast.Number(args[0]))));

    private CesqlFunction(string key, int arity, Func<Seq<CesqlValue>, CesqlResult> body) : this(key) =>
        (Arity, Body) = (arity, body);

    // Negative arity is VARIADIC, so `CONCAT` states its own shape rather than forcing a second column that every
    // fixed-arity row would answer identically.
    public int Arity { get; }

    public Func<Seq<CesqlValue>, CesqlResult> Body { get; }

    public bool Admits(int count) => Arity < 0 ? count > 0 : count == Arity;

    // Requested width CLAMPS into the text rather than refusing, because both affix rows are total by the
    // specification and a caller asking past the end is asking for the whole of it.
    static CesqlResult Slice(Seq<CesqlValue> args, bool fromStart) =>
        Sliced(CesqlCast.Text(args[0]), CesqlCast.Number(args[1]), fromStart);

    static CesqlResult Sliced(string text, int requested, bool fromStart) =>
        Math.Clamp(requested, 0, text.Length) switch {
            var width => CesqlResult.Of(fromStart ? text[..width] : text[^width..]),
        };

    // Out-of-range positions answer a FunctionEvaluationError beside the empty string, never a raise: the
    // specification's evaluator answers a value for every call it admits.
    static CesqlResult Substr(Seq<CesqlValue> args) =>
        Substrung(CesqlCast.Text(args[0]), CesqlCast.Number(args[1]), CesqlCast.Number(args[2]));

    static CesqlResult Substrung(string text, int at, int width) =>
        at >= 1 && width >= 0 && (at - 1) + width <= text.Length
            ? CesqlResult.Of(text.Substring(at - 1, width))
            : CesqlResult.Of(string.Empty).Fault(new CesqlFault.FunctionEvaluationError("SUBSTRING", "<range>"));
}

// --- [OPERATIONS] -----------------------------------------------------------------------

// Implicit casts seat one matrix entry per (from, to) the specification defines. A cast that cannot succeed
// answers the target type's ZERO beside a `CastError`, because the evaluator is total and a raise here would make one
// malformed attribute value darken an otherwise-matching subscription. Three NAMED crossings rather than one
// target-string switch: the target is a compile-time fact at every operand site, so a stringly dispatch would turn it
// into a runtime lookup with a fall-through arm no caller can see the shape of.
public static class CesqlCast {
    public static string Text(CesqlValue value) => value.Switch(
        text: static t => t,
        number: static n => n.ToString(CultureInfo.InvariantCulture),
        flag: static f => f ? "true" : "false");

    public static int Number(CesqlValue value) => value.Switch(
        text: static t => int.TryParse(t, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out int parsed) ? parsed : 0,
        number: static n => n,
        flag: static f => f ? 1 : 0);

    public static bool Flag(CesqlValue value) => value.Switch(
        text: static t => bool.TryParse(t, out bool parsed) && parsed,
        number: static n => n != 0,
        flag: static f => f);

    // Every operand takes these refusal-aware crossings: the projected value AND the fault a lossy crossing owes, so
    // an arm reading a number over unparseable text reports why rather than silently comparing against zero. Text is
    // total from all three spaces and therefore owes nothing.
    public static CesqlResult AsText(CesqlValue value) => CesqlResult.Of(Text(value));

    public static CesqlResult AsNumber(CesqlValue value) =>
        value is { IsText: true } text && !int.TryParse(text.AsText, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out _)
            ? CesqlResult.Of(0).Fault(new CesqlFault.CastError("String", "Integer"))
            : CesqlResult.Of(Number(value));

    public static CesqlResult AsFlag(CesqlValue value) =>
        value is { IsText: true } text && !bool.TryParse(text.AsText, out _)
            ? CesqlResult.Of(false).Fault(new CesqlFault.CastError("String", "Boolean"))
            : CesqlResult.Of(Flag(value));
}

// Compiled expressions form a recursive class-kind union whose cases nest the root, so depth growth is absorbed
// at the case that owns it and every consumer's dispatch stays total.
[Union]
public abstract partial record CesqlExpression {
    private CesqlExpression() { }

    public sealed record Literal(CesqlValue Value) : CesqlExpression;
    public sealed record Attribute(string Name) : CesqlExpression;
    public sealed record Unary(CesqlOperator Op, CesqlExpression Operand) : CesqlExpression;
    public sealed record Binary(CesqlOperator Op, CesqlExpression Left, CesqlExpression Right) : CesqlExpression;
    public sealed record Call(CesqlFunction Function, Seq<CesqlExpression> Arguments) : CesqlExpression;
    public sealed record Member(CesqlExpression Subject, Seq<CesqlExpression> Set, bool Negated) : CesqlExpression;

    // Attribute reads cross the branch owner's DECLARED roster, so an expression naming a row this estate never
    // declares answers `MissingAttributeError` rather than matching against whatever untyped string a foreign
    // producer happened to set — and each present value renders through the attribute's OWN `Format`, so the text an
    // expression compares is the exact text the wire carries.
    public CesqlResult Evaluate(CloudEvent envelope) => Switch(
        state: envelope,
        literal:   static (_, node) => CesqlResult.Of(node.Value),
        attribute: static (event_, node) => Read(event_, node.Name),
        unary:     static (event_, node) => node.Op.Apply(node.Operand.Evaluate(event_)),
        binary:    static (event_, node) => node.Op.Apply(node.Left.Evaluate(event_), node.Right.Evaluate(event_)),
        call:      static (event_, node) => Invoke(node, event_),
        member:    static (event_, node) => Contains(node, event_));

    static CesqlResult Read(CloudEvent envelope, string name) =>
        Optional(envelope.GetAttribute(name)).Bind(attribute => Optional(envelope[attribute]).Map(value => (attribute, value)))
            .Match(
                Some: held => CesqlResult.Of(held.attribute.Format(held.value)),
                None: () => CesqlResult.Of(string.Empty).Fault(new CesqlFault.MissingAttributeError(name)));

    // Arity refuses at the ROW, so a call the grammar admitted but the function cannot serve reports its own name
    // rather than raising out of a body sized for a different argument count.
    static CesqlResult Invoke(Call node, CloudEvent envelope) =>
        Applied(node.Function, node.Arguments.Map(argument => argument.Evaluate(envelope)));

    static CesqlResult Applied(CesqlFunction row, Seq<CesqlResult> evaluated) =>
        evaluated.Fold(Seq<CesqlFault>(), static (held, one) => held + one.Faults) switch {
            var faults when row.Admits(evaluated.Count) =>
                row.Body(evaluated.Map(static one => one.Value)) switch {
                    var answered => answered with { Faults = faults + answered.Faults },
                },
            var faults => new CesqlResult(false, faults.Add(new CesqlFault.FunctionEvaluationError(row.Key, "<arity>"))),
        };

    // Membership folds the SET onto the subject's own faults, so a set element that erred reports beside the verdict;
    // `NOT IN` inverts the bit alone and the accumulated evidence rides through untouched.
    static CesqlResult Contains(Member node, CloudEvent envelope) =>
        Membered(node.Subject.Evaluate(envelope), node.Set.Map(one => one.Evaluate(envelope)), node.Negated);

    static CesqlResult Membered(CesqlResult subject, Seq<CesqlResult> set, bool negated) =>
        set.Fold(new CesqlResult(false, subject.Faults),
                (held, one) => held.Join(one, CesqlCast.Flag(held.Value)
                    || string.Equals(CesqlCast.Text(subject.Value), CesqlCast.Text(one.Value), StringComparison.Ordinal)))
            switch {
                var folded => folded with { Value = negated ? !CesqlCast.Flag(folded.Value) : CesqlCast.Flag(folded.Value) },
            };
}
```

```csharp signature
// --- [TYPES] ----------------------------------------------------------------------------
// Operators as ROWS carrying their own total bodies, so the precedence table names a row and the evaluator invokes
// one — never a switch over a symbol string in two places that can disagree about what `%` does. Arithmetic rows
// read the checked guard once: a `MathError` beside a defined value is the specification's answer, and an escaping
// `OverflowException` under this branch's checked arithmetic is the deleted form.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CesqlOperator {
    public static readonly CesqlOperator And = new("AND", static (l, r) => Logical(l, r, static (a, b) => a && b));
    public static readonly CesqlOperator Or = new("OR", static (l, r) => Logical(l, r, static (a, b) => a || b));
    public static readonly CesqlOperator Xor = new("XOR", static (l, r) => Logical(l, r, static (a, b) => a ^ b));
    public static readonly CesqlOperator Equal = new("=", static (l, r) => l.Join(r, Same(l.Value, r.Value)));
    public static readonly CesqlOperator NotEqual = new("<>", static (l, r) => l.Join(r, !Same(l.Value, r.Value)));
    public static readonly CesqlOperator Less = new("<", static (l, r) => Compare(l, r, static (a, b) => a < b));
    public static readonly CesqlOperator LessEqual = new("<=", static (l, r) => Compare(l, r, static (a, b) => a <= b));
    public static readonly CesqlOperator Greater = new(">", static (l, r) => Compare(l, r, static (a, b) => a > b));
    public static readonly CesqlOperator GreaterEqual = new(">=", static (l, r) => Compare(l, r, static (a, b) => a >= b));
    public static readonly CesqlOperator Add = new("+", static (l, r) => Arith(l, r, "+", static (a, b) => (long)a + b));
    public static readonly CesqlOperator Subtract = new("-", static (l, r) => Arith(l, r, "-", static (a, b) => (long)a - b));
    public static readonly CesqlOperator Multiply = new("*", static (l, r) => Arith(l, r, "*", static (a, b) => (long)a * b));
    public static readonly CesqlOperator Divide = new("/", static (l, r) => Divided(l, r, "/"));
    public static readonly CesqlOperator Modulo = new("%", static (l, r) => Divided(l, r, "%"));
    public static readonly CesqlOperator Like = new("LIKE", static (l, r) => l.Join(r, Matches(CesqlCast.Text(l.Value), CesqlCast.Text(r.Value))));
    public static readonly CesqlOperator Not = new("NOT", static (l, _) => Admitted(l, CesqlCast.AsFlag) with { Value = !CesqlCast.Flag(l.Value) });
    public static readonly CesqlOperator Negate = new("NEG", static (l, _) => Arith(l, CesqlResult.Of(-1), "-", static (a, b) => (long)a * b));
    public static readonly CesqlOperator Exists = new("EXISTS", static (l, _) => l with { Value = l.Faults.Exists(static f => f is CesqlFault.MissingAttributeError) is false });

    private CesqlOperator(string key, Func<CesqlResult, CesqlResult, CesqlResult> apply) : this(key) => Body = apply;

    public Func<CesqlResult, CesqlResult, CesqlResult> Body { get; }

    public CesqlResult Apply(CesqlResult left, CesqlResult right) => Body(left, right);

    public CesqlResult Apply(CesqlResult operand) => Body(operand, CesqlResult.Of(0));

    // Equality is SAME-TYPE by the specification, so a `String` and an `Integer` compare through the cast matrix's
    // text projection rather than answering false on a type mismatch a producer never intended.
    static bool Same(CesqlValue left, CesqlValue right) =>
        left.IsNumber && right.IsNumber
            ? left.AsNumber == right.AsNumber
            : string.Equals(CesqlCast.Text(left), CesqlCast.Text(right), StringComparison.Ordinal);

    // ONE admission crossing every typed arm takes: the operand's own faults plus whatever its cast owed, carried
    // on the operand's own value, so a lossy text-to-number or text-to-boolean read reports its `CastError` rather than
    // comparing against a silent zero the specification never asked for.
    static CesqlResult Admitted(CesqlResult operand, Func<CesqlValue, CesqlResult> cast) =>
        operand.Join(cast(operand.Value), operand.Value);

    // Three logical rows and four relational rows are TWO folds over their own comparison: enumerating seven near
    // identical bodies is where an admission crossing lands on six of them and goes missing on the seventh.
    static CesqlResult Logical(CesqlResult left, CesqlResult right, Func<bool, bool, bool> fold) =>
        Admitted(left, CesqlCast.AsFlag).Join(Admitted(right, CesqlCast.AsFlag),
            fold(CesqlCast.Flag(left.Value), CesqlCast.Flag(right.Value)));

    static CesqlResult Compare(CesqlResult left, CesqlResult right, Func<int, int, bool> fold) =>
        Admitted(left, CesqlCast.AsNumber).Join(Admitted(right, CesqlCast.AsNumber),
            fold(CesqlCast.Number(left.Value), CesqlCast.Number(right.Value)));

    // Every arithmetic arm ADMITS both operands, computes WIDE, and re-admits the width: a result outside the 32-bit
    // range is a `MathError` carrying the saturated value, so the evaluator stays total where checked arithmetic
    // would raise.
    static CesqlResult Arith(CesqlResult left, CesqlResult right, string op, Func<int, int, long> fold) =>
        Widened(Admitted(left, CesqlCast.AsNumber), Admitted(right, CesqlCast.AsNumber), op,
            fold(CesqlCast.Number(left.Value), CesqlCast.Number(right.Value)));

    static CesqlResult Widened(CesqlResult left, CesqlResult right, string op, long wide) => wide switch {
        >= int.MinValue and <= int.MaxValue => left.Join(right, (int)wide),
        < 0 => left.Join(right, int.MinValue).Fault(new CesqlFault.MathError(op, "<int32-overflow>")),
        _ => left.Join(right, int.MaxValue).Fault(new CesqlFault.MathError(op, "<int32-overflow>")),
    };

    // Division by zero is the second `MathError` seat and answers zero, because the specification's evaluator
    // produces a value for every expression it admits.
    static CesqlResult Divided(CesqlResult left, CesqlResult right, string op) =>
        (Admitted(left, CesqlCast.AsNumber), Admitted(right, CesqlCast.AsNumber)) switch {
            (var l, var r) when CesqlCast.Number(right.Value) is 0 =>
                l.Join(r, 0).Fault(new CesqlFault.MathError(op, "<divide-by-zero>")),
            (var l, var r) when op is "/" =>
                l.Join(r, CesqlCast.Number(left.Value) / CesqlCast.Number(right.Value)),
            (var l, var r) => l.Join(r, CesqlCast.Number(left.Value) % CesqlCast.Number(right.Value)),
        };

    // `LIKE` is the specification's own two-wildcard grammar — `%` any run, `_` one glyph. Exemption: a span scan
    // is the kernel that grammar earns, and it is what keeps the compiled-once law true — a `Regex` built from the
    // pattern operand constructs an engine on EVERY event, which is exactly the per-event allocation the parser being
    // a value exists to foreclose. Backtracking is single-anchor, so a pattern of runs costs no exponential retry and
    // no caller injects an engine this grammar never defined.
    static bool Matches(ReadOnlySpan<char> subject, ReadOnlySpan<char> pattern) {
        int at = 0, cursor = 0, star = -1, mark = 0;
        while (at < subject.Length) {
            if (cursor < pattern.Length && (pattern[cursor] is '_' || pattern[cursor] == subject[at])) { at++; cursor++; continue; }
            if (cursor < pattern.Length && pattern[cursor] is '%') { star = cursor++; mark = at; continue; }
            if (star < 0) return false;
            cursor = star + 1;
            at = ++mark;
        }
        while (cursor < pattern.Length && pattern[cursor] is '%') cursor++;
        return cursor == pattern.Length;
    }
}

// --- [OPERATIONS] -----------------------------------------------------------------------

// Grammar builds ONCE as static values: a parser is immutable, so constructing one per subscription rebuilds the
// whole expression graph for a text this fabric parses at admission and never again.
public static class Cesql {
    // `Rec` is the ONE self-reference seat: a static field cannot read a field whose initializer has not run, so the
    // term parser defers to `Atom` and `Atom` defers back through the parenthesised arm — the recursion the grammar
    // genuinely has, declared once.
    static readonly Parser<char, CesqlExpression> Expression =
        ExpressionParser.Build(Rec(static () => Atom), [
            [Operator.Prefix(Unary(CesqlOperator.Negate, "-")), Operator.Prefix(Unary(CesqlOperator.Not, "NOT"))],
            [Operator.InfixL(Infix(CesqlOperator.Multiply, "*")), Operator.InfixL(Infix(CesqlOperator.Divide, "/")), Operator.InfixL(Infix(CesqlOperator.Modulo, "%"))],
            [Operator.InfixL(Infix(CesqlOperator.Add, "+")), Operator.InfixL(Infix(CesqlOperator.Subtract, "-"))],
            // Two-glyph relational spellings precede their one-glyph prefixes at the SAME level, since `Or` commits
            // to a branch that consumed input and `<` would otherwise swallow the head of `<=`.
            [Operator.InfixN(Infix(CesqlOperator.LessEqual, "<=")), Operator.InfixN(Infix(CesqlOperator.GreaterEqual, ">=")),
             Operator.InfixN(Infix(CesqlOperator.Less, "<")), Operator.InfixN(Infix(CesqlOperator.Greater, ">"))],
            [Operator.InfixN(Infix(CesqlOperator.NotEqual, "<>")), Operator.InfixN(Infix(CesqlOperator.Equal, "=")), Operator.InfixN(Infix(CesqlOperator.Like, "LIKE"))],
            [Operator.InfixL(Infix(CesqlOperator.And, "AND"))],
            [Operator.InfixL(Infix(CesqlOperator.Xor, "XOR"))],
            [Operator.InfixL(Infix(CesqlOperator.Or, "OR"))],
        ]);

    // Every terminal is `Labelled`, so a refusal names this grammar's own vocabulary rather than a character class;
    // `Try` guards each shared-prefix alternation, since `Or` does not backtrack a branch that already consumed input
    // — a function name and a bare attribute share every leading glyph.
    static readonly Parser<char, CesqlExpression> Atom =
        OneOf(
            Try(Call).Labelled("<function-call>"),
            Try(Rec(static () => Expression).Between(Char('(').Between(SkipWhitespaces), Char(')').Between(SkipWhitespaces))),
            Literal.Labelled("<literal>"),
            Member.Labelled("<attribute>"))
        .Between(SkipWhitespaces);

    // Literals close on the three value spaces: a single-quoted string (doubled quote escapes), a signed integer,
    // and the two boolean words. `Num` is the specification's own integer shape, so no second numeric grammar exists to
    // disagree with the 32-bit width `CesqlValue` declares.
    static readonly Parser<char, CesqlExpression> Literal =
        OneOf(
            Try(AnyCharExcept('\'').ManyString().Between(Char('\''))).Map(static text => (CesqlExpression)new CesqlExpression.Literal(text)),
            Try(Num).Map(static value => (CesqlExpression)new CesqlExpression.Literal(value)),
            Try(CIString("TRUE")).ThenReturn((CesqlExpression)new CesqlExpression.Literal(true)),
            Try(CIString("FALSE")).ThenReturn((CesqlExpression)new CesqlExpression.Literal(false)));

    // Attribute names follow the branch grammar's own alphabet, and the optional `IN`/`NOT IN` tail is the membership
    // arm — one parser, because the set test is a property of the SUBJECT expression rather than a second production
    // a caller could reach without one.
    static readonly Parser<char, CesqlExpression> Member =
        from name in LetterOrDigit.AtLeastOnceString().Labelled("<attribute-name>")
        from tail in OneOf(
            Try(CIString("NOT").Between(SkipWhitespaces).Then(CIString("IN"))).ThenReturn(true),
            Try(CIString("IN").Between(SkipWhitespaces)).ThenReturn(false)).Optional()
        from set in tail.HasValue
            ? Rec(static () => Expression).Separated(Char(',').Between(SkipWhitespaces))
                .Between(Char('(').Between(SkipWhitespaces), Char(')').Between(SkipWhitespaces)).Map(toSeq)
            : Return(Seq<CesqlExpression>())
        select tail.HasValue
            ? (CesqlExpression)new CesqlExpression.Member(new CesqlExpression.Attribute(name), set, tail.Value)
            : new CesqlExpression.Attribute(name);

    // Calls resolve their NAME through the function table at PARSE time, so an unknown function refuses the
    // subscription rather than answering `MissingFunctionError` on every event of a subscription that can never match
    // — the one diagnostic the table lets the grammar hoist to admission.
    static readonly Parser<char, CesqlExpression> Call =
        from name in Letter.AtLeastOnceString().Labelled("<function-name>")
        from args in Rec(static () => Expression).Separated(Char(',').Between(SkipWhitespaces))
            .Between(Char('(').Between(SkipWhitespaces), Char(')').Between(SkipWhitespaces))
        from resolved in Resolved(name)
        select (CesqlExpression)new CesqlExpression.Call(resolved, toSeq(args));

    // Table lookup narrows to a proved row before the parser branches, so the resolution reads its presence rather
    // than asserting a nullable the compiler never narrowed.
    static Parser<char, CesqlFunction> Resolved(string name) =>
        CesqlFunction.TryGet(name, out CesqlFunction? row) && row is CesqlFunction found
            ? Return(found)
            : Parser<char>.Fail<CesqlFunction>($"<unknown-function:{name}>");

    // ONE admission: an unparseable expression refuses the SUBSCRIPTION here, so no delivery ever evaluates a grammar
    // this fabric could not read, and the rendered `ParseError` names the offset and the expectation set the
    // `Labelled` terminals published rather than a bare failure. `Before(End)` is what makes a trailing fragment a
    // refusal instead of a silently truncated predicate that matches more than its author wrote.
    public static Fin<CesqlExpression> Compile(string text, Op key) =>
        Expression.Before(End).Parse(text).Match(
            success: static parsed => Fin.Succ(parsed),
            failure: error => Fin.Fail<CesqlExpression>(new CesqlFault.ParseError(error.RenderErrorMessage(), Some(key))));

    static Parser<char, Func<CesqlExpression, CesqlExpression, CesqlExpression>> Infix(CesqlOperator op, string spelling) =>
        Try(CIString(spelling)).Between(SkipWhitespaces).ThenReturn<Func<CesqlExpression, CesqlExpression, CesqlExpression>>(
            (left, right) => new CesqlExpression.Binary(op, left, right));

    static Parser<char, Func<CesqlExpression, CesqlExpression>> Unary(CesqlOperator op, string spelling) =>
        Try(CIString(spelling)).Between(SkipWhitespaces).ThenReturn<Func<CesqlExpression, CesqlExpression>>(
            operand => new CesqlExpression.Unary(op, operand));
}
```

## [05]-[RESEARCH]

(none)

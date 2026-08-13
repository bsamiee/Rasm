# [PERSISTENCE_VERSION_EGRESS]

`EgressPump` drains durable `OpLogEntry` rows past each subscription cursor, mints one envelope through the branch owner, folds every provider result into `DeliveryAck`, and advances only a confirmed contiguous prefix. `Subscription` instances ride `Binding` rows carrying modes, prefix, routing member, `dataref` policy, and `protocolsettings` roster as DATA, so a delivery target is a value a deployment authors. `(source, id)` keys replay beside the content-keyed `subject`; presence and awareness never enter this durable rail.

## [01]-[INDEX]

- [02]-[EGRESS_PUMP]: `EgressPump` drains one fold past each subscription cursor — profile lane gate, advance law, dead-letter and replay rows, `EgressReceipt` floor, 8270 band.
- [03]-[EGRESS_SINK]: `Egress.Envelope` mints the envelope every `Subscription` delivers, the `Binding` roster and its `ProtocolSettings` admission, and the `DeliveryAck` fold under the dedup, settlement-contract, and in-flight-bound columns.
- [04]-[SUBSCRIPTION_FILTER]: `FilterDialect` the seven-dialect delivery predicate, `Cesql` the table-driven expression owner, and the accumulating `CesqlFault` rail its evaluation returns.

## [02]-[EGRESS_PUMP]

- Owner: `EgressPump` the static surface owning the one drain bracket — notification wait, cursor read, windowed row drain, envelope mint, subscription delivery, ack fold, cursor advance, dead-letter capture; `DeadLetterRow` the typed dead-letter document (content key, sink, sequence, fault, attempt count) stored in the SAME Marten session so a dead-letter and its cursor state commit atomically, its whole table the `Version/retention#RETENTION_CLASSES` `evidence` class and therefore one `Store/provisioning#SERVER_EXTENSIONS` `RollingWindow` row; `EgressReceipt` the per-drain evidence implementing the kernel `IValidityEvidence`; `EgressFault` the 8270 band; `EgressPorts` the injected delegate frame (`Wait` binds `NpgsqlConnection.WaitAsync` with its bounded poll, then feed, coordination, the payload arrow, the propagator's carrier rendering, the selected `StoreProfile` both entries gate on, and the session-bound dead-letter triple — `Letters` reads, `DeadLetter` writes, `Retire` terminates) filled at the composition root.
- Entry: `EgressPump.Lane` spells this page's `Store/provisioning#SERVER_EXTENSIONS` `StoreProfile` lane token once, and BOTH drain entries open on `ports.Profile.Admits(EgressPump.Lane)` — a refusal is `EgressFault.LaneUnrealizable` returned before any wake, feed, delivery, or cursor read, because the embedded profile realizes NO egress lane and the absence belongs at this pump's own door rather than at a first publish against a store carrying no outbox relation at all; `public static StoreOptions Partition(StoreOptions opts)` publishes the `dead-letter` `Store/provisioning#SERVER_EXTENSIONS` `RollingWindow` mapping contribution the composition root folds over the spine seat; `public static IO<Fin<EgressReceipt>> Drain(Subscription sink, OutboxCursor cursor, EgressPorts ports, ProjectionContext frame)` is the one pump — it drains `ReplayWindow.DurableOps(cursor.Sequence, sink.Bind.Batch)` rows from the changefeed (the `Version/ledger` windowed-read case parameterized for this drain — never a third read surface), mints each row through `Egress.Envelope`, delivers through the subscription's own leg, folds every outcome to `DeliveryAck`, and advances the cursor through `Store/coordination` `OutboxAdvance(sink.Bind.Key, through)` ONLY past the contiguous `Persisted` prefix — the first `Indeterminate` holds the cursor at its predecessor (a held cursor re-drains, the binding's dedup absorbs the replay), a `Refused` writes the `DeadLetterRow` and the drain continues past it; `public static IO<Fin<EgressReceipt>> Replay(Subscription sink, EgressPorts ports, ProjectionContext frame)` loads that subscription's letters through `EgressPorts.Letters` at its own batch width and re-delivers them by content key through the SAME mint/deliver/ack fold — replay is the pump re-parameterized, never a second delivery path, and it takes the drain's own three arguments because the loader is the read half of the pair `DeadLetter`/`Retire` write.
- Auto: the drain is one fold per batch — mint, filter, deliver — folded left with the contiguous-prefix advance accumulator, so the drain hands rows to each leg in sequence order and a mid-batch refusal never advances past unconfirmed work; a row the subscription's own `filters` withhold settles as delivered-and-filtered rather than as an ack, because a predicate answering false is a routing decision the receipt counts and never a transport outcome; an envelope the branch owner REFUSES to mint is poison by construction and folds `Refused` into the letter table, since a malformed grammar value cannot become well-formed on a later attempt; what a binding then preserves is its own engine's answer, not the envelope's — `partitionkey` reaches a real routing key on exactly two rows (Kafka `Message.Key`, Pulsar `MessageMetadata.Key`/`OrderingKey`), while NATS orders per subject, RabbitMQ per queue, MQTT per topic, and AMQP per link, none of which expose a key member at all, so per-entity order on those rows holds only where one entity's rows share one subject, queue, topic, or link (`#EGRESS_SINK`) and a blanket per-entity-order claim over the whole family reads as a guarantee six engines never made; the pump wakes on the coordination `pg_notify('rasm_outbox', sink)` channel through `NpgsqlConnection.WaitAsync` on an otherwise-idle connection, with the bounded poll as the correctness floor (a missed NOTIFY costs latency, never a lost row — the cursor law owns correctness); the HTTP row's `DeliveryUnconfirmed` reconciliation re-reads `net._http_response` by request-id on the NEXT drain, so a PENDING response resolves without a dedicated poller; a crash between delivery and advance re-drains the suffix and every binding's dedup column states what absorbs it (`#EGRESS_SINK`); dead-letter replay decrements nothing — the receipt's conservation fold proves `delivered + filtered + held + deadLettered == drained` on every drain.
- Receipt: a drain rides `store.egress.drain` carrying the sink, the from/through sequences, and the delivered/duplicate/held/dead-lettered counts; a dead-letter rides `store.egress.deadletter` carrying the content key and the fault; a replay rides `store.egress.replay`; each settled drain receipt fires the `rasm.persistence.egress.delivered` observe point (`Store/observability#HOOK_RAIL`) as a composition-root tap on the drain outcome, never an emit call inside the fold.
- Packages: Npgsql (`NpgsqlConnection.Notification`/`WaitAsync` — the pump wake), Marten (`IDocumentSession.Store`/`SaveChangesAsync` — the dead-letter document; `StoreOptions.Schema.For<T>().PartitionOn` through `RollingWindow.Declare` — its rolling window), Rasm (`IValidityEvidence`/`ValidityClaim`), Microsoft.Extensions.Compliance.Redaction (`IRedactorProvider.GetRedactor(DataClassificationSet)` — the classified-field gate before the boundary), LanguageExt.Core, NodaTime, Thinktecture.Runtime.Extensions, BCL inbox.
- Growth: a new delivery target is one `Subscription` value over an existing `Binding` row and one `outbox_cursor` row minted on first drain — zero pump edits and zero new types; a new transport is one `Binding` row carrying its modes, prefix, routing member, `protocolsettings` roster, and `dataref` policy; a new drain policy (batch width, wake channel, payload arrow) is one `EgressPorts`/subscription value; zero new surface — a per-sink pump, a second delivery path for replay, a fire-and-forget HTTP post, a presence row in the CDC drain, a lane gate seated at a caller instead of these two entries, or a CDC poller beside the changefeed is the deleted form because the pump is one fold, replay is the same fold, the advance law owns the cursor, and the durable lanes are the only drain source.
- Boundary: the pump drains the durable outbox — `Family.Durable` lanes past the per-subscription cursor — and the presence/awareness lane (`durable: false`) NEVER enters the envelope (the lossy `DrainSurface` is its only transport); the cursor-advance CAS failure is `CoordinationFault.OutboxDrain` raised by the coordination store (the fenced write is its rail), while every delivery fault is THIS band — `DeadLetter` the poisoned entry, `SinkRefused` the sink-level refusal, `CursorStall` the held cursor evidence, `DeliveryUnconfirmed` the pg_net PENDING/ERROR unconfirmed state the advance law reconciles; the payload arrow redacts and frames BEFORE the mint (`ErasingRedactor` the fail-closed fallback) so an out-of-authority payload crosses masked, never raw, and the grade it answers is the `dataclassification` the mint stamps; caller cancellation passes through untyped; the wire-native row hands bytes to the AppHost `OutboundHop` keyed pipeline and reads its delivery-honesty policy — Persistence never owns that channel; the letter table retires by partition drop, not by row sweep, so a letter neither `Retire` nor `Replay` ever consumed leaves at its window's trailing edge as one receipted `Version/retention#SWEEP_AND_GC` `DropPartition` and an unbounded letter table has no reachable state.

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

// The payload arrow's WHOLE answer, minted once per row and read by every subscription draining it: the framed
// body, the handling grade the redaction pass settled, and the serdes arrow's OWN content type, registry binding,
// and breaking generation. One arrow instead of a redactor beside a serializer beside a content-type literal,
// because the same pass that frames the bytes is the only pass that knows what framed them — a `datacontenttype`
// chosen anywhere else describes the mint site's guess, and an unconditional `application/octet-stream` over an
// Avro, JSON, or registry-framed Protobuf body makes every consumer decode by convention. `Major` rides beside
// `Schema` for the same reason the fabric binds them: the `type` major moves WITH the registry version rather
// than beside it, so a literal at the mint site freezes every announcement at one generation while the schema
// this arrow resolved moves underneath it. `Residence` fills where the drain externalizes the body, and it is the
// ONLY slot a subscription-scoped pass writes — the residence key names bytes a store already holds.
public readonly record struct PayloadFrame(
    ReadOnlyMemory<byte> Body, DataGrade Grade, string ContentType, Option<Uri> Schema, int Major, Option<UInt128> Residence = default);

// `EgressPorts` is the injected delegate frame the composition root fills: provider clients, the coordination advance arrow,
// the payload arrow, the dead-letter store/retire arrows, and the SELECTED `StoreProfile` — values on a
// Persistence-owned shape, never an AppHost type ([A.1]). `Profile` seats first because both entries read it
// before any other member, and it arrives as the deployment's own selected row rather than a boolean a caller
// computed, so the gate reads the same `Lanes` set the provisioning fold verified. `DeadLetter`/`Retire`/`Letters` close over the SAME Marten `IDocumentSession` as the
// cursor state (`Store` then `SaveChangesAsync` with the drain), so a letter and its drain commit atomically —
// `Letters` is the READ half that pairs them: a letter set is loaded by sink under the same tenant transaction
// that will retire or re-letter it, which is what makes `Replay` a closed fold rather than a surface whose
// caller has to find its own input and can therefore hand it letters from another sink or another tenant; `Frame`
// masks the row's classified payload fields (built from `IRedactorProvider.GetRedactor(DataClassificationSet)`,
// `ErasingRedactor` the fail-closed fallback) and hands back the whole `PayloadFrame`. `Reside` is the
// `Store/blobstore#OBJECT_STORE` residence port a body past the row's `dataref` threshold crosses, and it is a
// REQUIRED slot: an unbound residence cannot construct these ports at all, so a deployment carrying no object
// store refuses at composition rather than shipping a reference nothing resolves. `Carrier` renders a
// continued `ActivityContext` onto the KERNEL `TraceCarrier` at the propagator owning that format — the AppHost
// `TraceContext` adapter — so version byte, sampled flag, and every tracestate mutation reach the mint as an
// admitted value; a `$"00-{traceId}-{spanId}-01"` interpolation on this side re-mints the wire format the
// propagator owns and freezes both version and flag byte against a spec revision, and a bare
// `(string, Option<string>)` tuple beside the kernel carrier is a second spelling of one value.
public sealed record EgressPorts(
    StoreProfile Profile,
    Func<IO<Unit>> Wait,
    Func<CoordinationOp, Option<LeaseToken>, IO<Fin<CoordinationReceipt>>> Coordinate,
    Func<ReplayWindow, IO<Seq<OpLogEntry>>> Feed,
    Func<OpLogEntry, PayloadFrame> Frame,
    Func<PayloadFrame, IO<UInt128>> Reside,
    Func<ActivityContext, TraceCarrier> Carrier,
    Func<SinkKey, int, IO<Seq<DeadLetterRow>>> Letters,
    Func<DeadLetterRow, IO<Unit>> DeadLetter,
    Func<DeadLetterRow, IO<Unit>> Retire);

// `EgressReceipt` rides the kernel validity floor as per-drain evidence: conservation is the fold — every drained row is
// delivered, filtered, held, or dead-lettered, exactly once ([C]). `Filtered` is a DELIVERY-SIDE verdict the
// subscription's own predicate reached, so it settles the row and advances the cursor exactly as a `Persisted`
// does; folding it into `Delivered` would report traffic to a transport that received nothing.
public sealed record EgressReceipt(SinkKey Sink, long From, long Through, int Drained, int Delivered, int Duplicates, int Filtered, int Held, int DeadLettered, Duration Elapsed, Instant At, CorrelationId Correlation) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.Of(Through >= From),
        ValidityClaim.CountExactly(Delivered + Filtered + Held + DeadLettered, Drained),
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

    // Admission-side refusal beside the four delivery outcomes: a `protocolsettings` slice missing a required
    // key or carrying one the binding row never declares refuses the SUBSCRIPTION, so a governance value no leg
    // reads can never reach a deployment that believes it configured something.
    public sealed record SettingsRejected(string Binding, string Detail, Option<Op> Key = default) : EgressFault;

    public override int Code => FaultBand.Egress + Switch(
        deadLetter:          static _ => 1,
        sinkRefused:         static _ => 2,
        cursorStall:         static _ => 3,
        deliveryUnconfirmed: static _ => 4,
        laneUnrealizable:    static _ => 5,
        settingsRejected:    static _ => 6);

    public override string Message => Switch(
        deadLetter:          static c => $"<dead-letter:{c.Sink.Value}:{c.ContentKey:x32}>:{c.Detail}",
        sinkRefused:         static c => $"<sink-refused:{c.Sink.Value}>:{c.Detail}",
        cursorStall:         static c => $"<cursor-stall:{c.Sink.Value}@{c.Held}>",
        deliveryUnconfirmed: static c => $"<delivery-unconfirmed:{c.Sink.Value}#{c.RequestId}>",
        laneUnrealizable:    static c => $"<lane-unrealizable:{c.Sink.Value}:{c.Lane}>",
        settingsRejected:    static c => $"<settings-rejected:{c.Binding}>:{c.Detail}");

    public override string Category => Switch(
        deadLetter:          static _ => "DeadLetter",
        sinkRefused:         static _ => "Sink",
        cursorStall:         static _ => "Cursor",
        deliveryUnconfirmed: static _ => "Unconfirmed",
        laneUnrealizable:    static _ => "Profile",
        settingsRejected:    static _ => "Settings");

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
    static IO<Fin<EgressReceipt>> Unrealizable(Subscription sink) =>
        IO.pure(Fin<EgressReceipt>.Fail(new EgressFault.LaneUnrealizable(sink.Bind.Key, Lane)));

    // `Partition` publishes the letter table's mapping contribution the composition root folds over the
    // `Element/graph#STREAM_GRAIN` spine seat: the policy is the `Store/provisioning#SERVER_EXTENSIONS` `RollingWindow` `dead-letter` row and
    // this publishes only the duplicated key, so a `PartitionOn` carrying its own period literals is the deleted
    // form. The contribution seats HERE because the S0 spine may not name this document type.
    public static StoreOptions Partition(StoreOptions opts) =>
        RollingWindow.DeadLetter.Declare<DeadLetterRow>(opts, static row => row.Window);

    // ONE drain fold: windowed rows -> envelope mint -> subscription filter -> delivery -> DeliveryAck ->
    // contiguous-prefix cursor advance. The first Indeterminate HOLDS the cursor (re-drain; the binding's dedup
    // absorbs the replay); a Refused PERSISTS its DeadLetterRow through the session-bound port BEFORE counting,
    // and the cursor advances past it — the durable letter, not the cursor, owns the poisoned entry from there.
    public static IO<Fin<EgressReceipt>> Drain(Subscription sink, OutboxCursor cursor, EgressPorts ports, ProjectionContext frame) =>
        ports.Profile.Admits(Lane) ? Drained(sink, cursor, ports, frame) : Unrealizable(sink);

    static IO<Fin<EgressReceipt>> Drained(Subscription sink, OutboxCursor cursor, EgressPorts ports, ProjectionContext frame) =>
        from mark in IO.lift(frame.Mark)
        from _ in ports.Wait()
        from rows in ports.Feed(ReplayWindow.DurableOps(cursor.Sequence, sink.Bind.Batch))
        from folded in rows.FoldM(
            (Through: cursor.Sequence, Delivered: 0, Duplicates: 0, Filtered: 0, Held: 0, Dead: 0, Open: true),
            (state, row) => !state.Open
                ? IO.pure(state with { Held = state.Held + 1 })
                // The mint is a rail, so a row whose grammar the branch owner refuses letters HERE rather than
                // reaching a transport that would accept the malformed value: a refused mint cannot succeed on a
                // later attempt, which is exactly the poison the letter table exists for. `Matches` runs after the
                // mint because every dialect reads admitted ATTRIBUTES, so a filter and a broker see one value.
                : Egress.Envelope(row, sink, ports).Bind(minted => minted.Match(
                    Fail: error => Lettered(row, sink, ports, frame, error.Message).Map(_ => state with { Through = row.Sequence, Dead = state.Dead + 1 }),
                    Succ: envelope => !sink.Matches(envelope).Delivers
                        ? IO.pure(state with { Through = row.Sequence, Filtered = state.Filtered + 1 })
                        : sink.Deliver(envelope, row).Bind(ack => ack.Switch(
                            persisted:     p  => IO.pure(state with { Through = row.Sequence, Delivered = state.Delivered + 1, Duplicates = state.Duplicates + (p.Duplicate ? 1 : 0) }),
                            indeterminate: _  => IO.pure(state with { Held = state.Held + 1, Open = false }),
                            refused:       rf => Lettered(row, sink, ports, frame, rf.Detail).Map(_ => state with { Through = row.Sequence, Dead = state.Dead + 1 })))))).As()
        from advance in folded.Through > cursor.Sequence
            ? ports.Coordinate(new CoordinationOp.OutboxAdvance(sink.Bind.Key, folded.Through), sink.Bind.Held)
            : IO.pure(Fin<CoordinationReceipt>.Succ(default!))
        let receipt = new EgressReceipt(sink.Bind.Key, cursor.Sequence, folded.Through, rows.Count, folded.Delivered, folded.Duplicates, folded.Filtered, folded.Held, folded.Dead, frame.Elapsed(mark), frame.Now(), frame.Correlation)
        select advance.Match(Succ: _ => Fin<EgressReceipt>.Succ(receipt), Fail: error => Fin<EgressReceipt>.Fail(error));

    // `Attempts: 1` is the MEASURED first attempt, not a filled slot: this fold is the only site a letter is
    // minted at and the row it letters was offered exactly once. Every later count comes from `Replay`'s own
    // `Attempts + 1`, so the column is monotone from its first write and no arm publishes a count no delivery
    // produced. ONE lettering seat serves the refused mint and the refused delivery, because both are the same
    // terminal verdict over one row and a second minting site would fork the attempt column's monotonicity.
    static IO<Unit> Lettered(OpLogEntry row, Subscription sink, EgressPorts ports, ProjectionContext frame, string detail) =>
        ports.DeadLetter(new DeadLetterRow(row.ContentKey, sink.Bind.Key, row.Sequence, detail, Attempts: 1, frame.Now()));

    // Replay IS the drain fold re-parameterized over the letter set — never a second delivery path. The letter
    // set is READ here through `ports.Letters` at the sink's own batch width rather than handed in, so replay
    // takes the same three arguments the drain takes and no caller can pair a sink with another sink's or
    // another tenant's letters. Each letter re-reads its row through the ONE windowed feed (the singleton
    // window at Sequence-1), re-delivers through the same envelope/leg, and a Persisted retires the letter; a
    // still-refusing row re-letters with Attempts+1 — attempts are MONOTONE by construction, the count is the
    // replay schedule's gate, and `Retire` is the one terminal, so no reset arrow exists to fabricate a fresh
    // budget for a poison row; a vanished row (retention-swept) retires as Held — the conservation fold closes
    // over letters exactly as the drain closes over rows.
    public static IO<Fin<EgressReceipt>> Replay(Subscription sink, EgressPorts ports, ProjectionContext frame) =>
        ports.Profile.Admits(Lane) ? Replayed(sink, ports, frame) : Unrealizable(sink);

    static IO<Fin<EgressReceipt>> Replayed(Subscription sink, EgressPorts ports, ProjectionContext frame) =>
        from mark in IO.lift(frame.Mark)
        from letters in ports.Letters(sink.Bind.Key, sink.Bind.Batch)
        from folded in letters.FoldM(
            (Delivered: 0, Duplicates: 0, Filtered: 0, Held: 0, Dead: 0),
            (state, letter) =>
                from rows in ports.Feed(ReplayWindow.DurableOps(letter.Sequence - 1, 1))
                let found = rows.Filter(r => r.ContentKey == letter.ContentKey).Head
                from next in found.Match(
                    // A letter whose envelope still refuses to mint re-letters on the SAME monotone attempt
                    // column as a refusing delivery, so a poisoned grammar value and a poisoned transport share
                    // one budget and one terminal rather than one of them looping outside the schedule.
                    Some: row => Egress.Envelope(row, sink, ports).Bind(minted => minted.Match(
                        Fail: error => ports.DeadLetter(letter with { Fault = error.Message, Attempts = letter.Attempts + 1, At = frame.Now() }).Map(_ => state with { Dead = state.Dead + 1 }),
                        Succ: envelope => !sink.Matches(envelope).Delivers
                            ? ports.Retire(letter).Map(_ => state with { Filtered = state.Filtered + 1 })
                            : from ack in sink.Deliver(envelope, row)
                              from settled in ack.Switch(
                                persisted:     p  => ports.Retire(letter).Map(_ => state with { Delivered = state.Delivered + 1, Duplicates = state.Duplicates + (p.Duplicate ? 1 : 0) }),
                                indeterminate: _  => IO.pure(state with { Held = state.Held + 1 }),
                                refused:       rf => ports.DeadLetter(letter with { Fault = rf.Detail, Attempts = letter.Attempts + 1, At = frame.Now() }).Map(_ => state with { Dead = state.Dead + 1 }))
                              select settled)),
                    None: () => ports.Retire(letter).Map(_ => state with { Held = state.Held + 1 }))
                select next).As()
        select Fin<EgressReceipt>.Succ(new EgressReceipt(sink.Bind.Key, 0, 0, letters.Count,
            folded.Delivered, folded.Duplicates, folded.Filtered, folded.Held, folded.Dead, frame.Elapsed(mark), frame.Now(), frame.Correlation));
}
```

| [INDEX] | [POLICY]      | [VALUE]                                        | [BINDING]                                                        |
| :-----: | :------------ | :--------------------------------------------- | :--------------------------------------------------------------- |
|   [01]  | drain source  | `ReplayWindow.DurableOps` past the sink cursor | one windowed read (ledger); presence never enters                |
|   [02]  | advance law   | contiguous `Persisted` prefix only             | Indeterminate holds; Refused dead-letters and continues          |
|   [03]  | replay        | `Letters` loads, the same fold re-delivers     | monotone `Attempts`; `Retire` terminal; no second delivery path  |
|   [04]  | wake          | `WaitAsync` on `rasm_outbox` + bounded poll    | NOTIFY is latency; the poll floor owns correctness               |
|   [05]  | payload arrow | `Frame` before the mint                        | fail-closed `ErasingRedactor`; grade, media, and schema as data  |
|   [06]  | filter        | the subscription's own AND-set, post-mint      | a withheld row settles and advances; it is no transport outcome  |
|   [07]  | receipt floor | conservation `ValidityClaim.All` fold          | delivered + filtered + held + dead == drained, once ([C])        |

## [03]-[EGRESS_SINK]

- Owner: `Egress.Envelope` the one mint of an `OpLogEntry` — composing `Rasm/Domain/event#ENVELOPE_MINT` `EventEnvelope.Mint`, so the branch owner's `Validate()` funnel runs on every projected row and the projection returns `IO<Fin<CloudEvent>>` — the residence write a body past the row's `dataref` threshold takes is the one effect a mint carries, and it lands BEFORE the envelope publishes the address; `Binding` the `[SmartEnum<string>]` transport roster carrying each transport's content modes, attribute prefix, routing member, `protocolsettings` key roster, `dataref` policy, pushdown verdict, and honest degrade as COLUMNS; `ProtocolSettings` the admitted per-subscription slice over that roster; `Subscription` the delivery instance pairing a binding row, its settings, its filter set, and its `SinkBinding`; `DeliveryAck` the one `[Union]` every provider outcome folds to at its own boundary — a raw `PubAckResponse`/`DeliveryResult`/`MessageId` never crosses into the pump; `SinkBinding.Watch` the one cell every leg consults before folding, because five of these engines publish a delivery failure on a surface the awaited return cannot reach and a leg reading only its await is blind to exactly the faults that matter.
- Entry: `Egress.Envelope(row, sink, ports)` mints on the `IO` rail; `Subscription.Deliver(envelope, row)` resolves the bound leg; `Subscription.Matches(envelope)` answers the filter AND-set; `ProtocolSettings.Admit(binding, settings, key)` is the ONE admission every subscription crosses at composition.
- Cases: `http` — enqueue `net.http_post(url, body, headers + idempotency-key = the envelope's own `(source, id)`)` returning the `bigint` request-id, the structured body arriving from the branch owner's ONE `EventEnvelope.Encode` (a body past the row's `dataref` threshold externalizes instead, so no leg holds a multi-megabyte payload to encode and the streamed base64 writer this fence once carried is deleted whole), fold `net.http_response_result` on the NEXT drain: `SUCCESS` → `Persisted`, `PENDING` → `Indeterminate` (`DeliveryUnconfirmed` evidence), `ERROR`/timeout → `Refused`; a pg_net UNLOGGED-table crash loses response rows, so the held cursor re-posts under the SAME idempotency-key header — receiver-side dedup, the row's honest stance. `nats` — `INatsJSContext.Publish` with `NatsHeaders["Nats-Msg-Id"]` carrying the envelope `id` beside the `traceparent`/`tracestate`/`baggage` header rows the AppHost `TraceContext` carrier adapter stamps, so every JetStream delivery joins the drain trace exactly as the Kafka leg's instrumented producer does; the `TryPublishAsync` ROP form answers `NatsResult<PubAckResponse>` and the fold reads BOTH of its error surfaces, because they mean different things and only one of them is a transport fault: `NatsResult.Error` is the publish never reaching the stream (`Indeterminate` — the held cursor re-drives into the dedup window), while `PubAckResponse.Error` rides a SUCCESSFUL result and is the stream's own rejection (`Refused`), so folding the result alone reports `Persisted` for an ack the stream refused; `PubAckResponse.Duplicate` is the dedup-window replay absorbed (`Persisted(Duplicate: true)`, `api-nats` JETSTREAM_DELIVERY_ACK). Batch drains pipeline through `PublishConcurrentAsync` and settles each `NatsJSPublishConcurrentFuture.GetResponseAsync` in offer order, so the window's round trips overlap while the contiguous-prefix advance still reads them sequentially — the one publish shape where this client's ack is decoupled from its send. `kafka` — `cloudEvent.ToKafkaMessage(ContentMode.Binary, formatter)` (attributes ride `ce_*` headers, `partitionkey` projects onto `Message.Key`, the ONE key member that makes per-entity order real here); the leg's `ProducerConfig` pins `EnableIdempotence = true` with `Acks.All`, because the dedup column claims broker-side suppression of producer retries and an unset flag leaves that claim unconfigured; the producer builds through `AsInstrumentedProducerBuilder` under `ConfluentKafkaInstrumentedProducerBuilderOptions` (`EnableTraces`/`EnableMetrics`), so delivery spans and client meters continue the envelope `traceparent` from drain to broker ack; `Awaited` folds the awaited `ProduceAsync` broker ack (`Status == Persisted` → `Persisted`, `NotPersisted`/`PossiblyPersisted` → `Indeterminate`) and a caught `ProduceException` through `DeliveryAck.FromError` on `Error.IsFatal` — `Error.IsRetriable` is INTERNAL, so `IsFatal` is the client's only public discriminator and a non-fatal produce fault re-drives rather than dead-letters; `ReadCommitted` brackets the publish with `InitTransactions` → `BeginTransaction` → `CommitTransaction` so `isolation.level=read_committed` consumers never observe an aborted record, and the pump's own close drains through `Flush(TimeSpan)` so a bounded shutdown lands what the produce queue already holds. Kafka transactions cannot commit the PostgreSQL outbox cursor; every mode remains at-least-once across that boundary and content-key consumer dedup absorbs a crash after broker persistence but before cursor advance. `rabbitmq` — `CreateChannelOptions(publisherConfirmationsEnabled: true, publisherConfirmationTrackingEnabled: true)` then awaited `BasicPublishAsync(exchange, routingKey, mandatory: true, properties, body, token)` (the await IS the confirm, and a nack throws under tracking): completion → `Persisted`, a caught nack → `Refused`. `mandatory: true` is load-bearing rather than decorative — an unroutable message under `mandatory: false` is discarded by the broker while the confirm still ACKS, so the leg reports `Persisted` for a row that reached no queue; the `BasicReturnEventArgs` the flag unlocks rides the row's `Watch` cell. `BasicProperties.DeliveryMode = DeliveryModes.Persistent` pins the message durable, since a durable quorum queue still loses a `Transient` message on broker restart and the queue's own durability never covers its contents. `pulsar` — `ISend.Send(metadata, payload)` → `MessageId` (`LedgerId:EntryId:Partition:BatchIndex`, the whole receipt: this client publishes NO status enum beside it) → `Persisted`; `MessageMetadata.SequenceId` carries the content key's low 64 bits as the broker's own dedup input (a custom property is a payload the broker never deduplicates on), `MessageMetadata.Key` carries `partitionkey` for routing, and `OrderingKey` carries the same `EntityKey` bytes so a `KeyShared` subscription keeps per-entity order across a rebalance; the producer builds under `ProducerAccessMode.WaitForExclusive` so one WAL-leader producer per topic is elected and a loser's `ProducerFencedException` re-elects rather than double-writes, and its `ISchema<T>` is the required `Schema.ByteArray` over the binary envelope — a producer built with no schema is refused by the client. `wirenative` — Persistence writes `MessageExtensions.WriteLengthPrefixedTo` bytes onto the AppHost `OutboundHop` keyed pipeline and folds the hop's delivery-honesty verdict; the gRPC channel is AppHost-owned. `redis` — await `StreamAdd(stream, fields, StreamIdempotentId(ContentKey-hex), trimMode: StreamTrimMode.Acknowledged)`; a returned stream id is `Persisted`, and a transport ambiguity is `Indeterminate`. Downstream consumers own `StreamReadGroup`/`StreamAcknowledge`; their independent group cursor never governs the PostgreSQL outbox cursor. `amqp` — the `AMQP 1.0` native binding DISTINCT from the 0-9-1 `rabbitmq` row (the two protocols share no message type, so a re-bind of the `RabbitMq` `Deliver` leg through this binding is structurally impossible and the transport family closes as two rows over the one envelope): `cloudEvent.ToAmqpMessageWithUnderscorePrefix(ContentMode.Binary, formatter)` maps the envelope onto AMQPNetLite's `Amqp.Message` with `cloudEvents_type`/`cloudEvents_source` application properties a header-filtering broker routes on, delivered through awaited `SenderLink.SendAsync(message, timeout)` inside the row's OWN bounded in-flight window — settled disposition → `Persisted`, transport ambiguity → `Indeterminate`, broker rejection → `Refused` (`AmqpException` over a `Released`/`Rejected` outcome, the awaited form raising it rather than reporting a status value); replay absorbs receiver-side on the CloudEvents `id`. That window is the `[BOUND]` column's whole AMQP value BECAUSE the client publishes no sender-side credit member: the PEER grants credit on its `Flow`, `ReceiverLink.SetCredit` has no `SenderLink` counterpart, and the callback form `Send(message, callback, state)` appends to an unbounded internal list the moment credit is absent, growing managed memory no fence member can read or cap — so that callback form is the DELETED form here and the awaited pair is the only admitted send. `InFlight` sizes a bounded `Channel` under `BoundedChannelFullMode.Wait`, offered in drain order and settled in OFFER order exactly as the NATS `PublishConcurrentAsync` row settles its futures, so the window's round trips overlap while the contiguous-prefix advance still reads them sequentially; `Wait` is the one admissible full mode because every drop mode discards a durable row the `EgressReceipt` conservation fold carries no arm to express, and the doctrine's loss column is empty for exactly that reason. This binding EXCLUDES `datacontenttype` from the application-property map and folds it into `Properties.ContentType` instead, so a broker header-filter selects on `cloudEvents_type`/`cloudEvents_source` and never on content type; `Uri` attributes serialize through `ToString()` and timestamps through `UtcDateTime`, which is why the envelope's `Time` crosses as UTC with its offset dropped and a consumer reconstructing local wall time reads the stamp, never the property. `clickhouse` (the table IS `Query/columnar#ANALYTICS_RESIDENCE` `WarehouseSchema.Table` with its `WarehouseSchema.Columns` roster — writer and reader share ONE typed row vocabulary, never two independently-authored shapes) — `ClickHouseClient.InsertBinaryAsync` (the pooled RowBinary ingest rail) with the `insert_deduplication_token` server setting = ContentKey-hex riding the `InsertOptions`/`QueryOptions` custom-settings row (the producer-supplied dedup id ClickHouse deduplicates replays on — exactly the content-key dedup stance the envelope law demands; `BeginDbTransaction` throws `NotSupportedException`, so the token IS the sink's whole dedup story), the awaited insert completion → `Persisted`, a transient server/connection fault → `Indeterminate` (the held cursor re-drives under the SAME token, absorbed), a schema/table rejection → `Refused`; the billion-row fleet-analytics lane whose read side is the `Query/columnar#ANALYTICS_RESIDENCE` `Residence.Fleet` row. Its `Watch` cell is the one row on this family standing behind NO event: this driver declares zero events on its connection, never raises the inherited `DbConnection.StateChange` (its backing state field takes a bare write on open and on close, and `OnStateChange` is called nowhere in the assembly), and ships no pool type at all — `ClickHouseConnectionFactory` is a `DbProviderFactory`, never a pool — so `ClickHouseConnection.State` echoes only an explicit `Open`/`Close` this fence already made and reading it answers nothing about the server. That cell is fed by an ACTIVE probe instead: the composition root runs `ClickHouseClient.PingAsync(QueryOptions?, CancellationToken)` on its own cadence and folds a false answer or a thrown `ClickHouseServerException` into the same cell every event-bearing row writes, so the `Watch` arrow holds ONE shape across the whole family and only this row's producer differs. `ClickHouse.Driver.Diagnostic.ClickHouseDiagnosticsOptions` and `TraceHelper` carry the `ActivitySource` name, the SQL-capture gate, and a `System.Net` trace toggle — telemetry configuration, never a fault signal — so neither feeds the cell; `ClickHouseBulkCopy.BatchSent` is the assembly's ONE event and stays declined, because it rides an `[Obsolete]` type this leg never constructs, its `RowsWritten` reports a cumulative row total rather than a per-envelope disposition, and the admitted `InsertBinaryAsync` rail publishes no progress callback beside it — settlement stays the awaited completion. `mqtt` — the BRANCH-OWNED MQTT 5.0 binding, because the published `CloudNative.CloudEvents.Mqtt` package compiles against a retired MQTTnet carrier shape and reaches structured mode alone, while the specification's MQTT binding defines BOTH content modes: binary mode writes each attribute as an UNPREFIXED v5 User Property (the one binding whose attribute names carry no prefix at all — `ce-` is HTTP's and `ce_` Kafka's, and a leg reusing either here publishes names no conforming peer resolves) with `Data` alone in the payload, and structured mode packs the whole envelope through `EventEnvelope.Encode`, whose returned `EventFrame` carries the body beside the framing this leg stamps onto `MqttApplicationMessage.ContentType`; the binary leg reads the same `PayloadFrame.ContentType` the mint stamped, so a broker filtering on topic alone and a consumer parsing by declaration both hold. The per-subscription `IMqttClient` is `MqttClientFactory.CreateMqttClient()`-minted (per-instance, disposed with the subscription — no host-wide singleton) under `MqttClientOptionsBuilder.WithProtocolVersion(MqttProtocolVersion.V500)` so the v5 `UserProperties` carrier is live (every v5 field drops SILENTLY under `V311` — no throw, no reason code, so the version pin is what keeps the attribute set, the trace pair, and the expiry from vanishing, and it is why the `V311` row of `[06]-[BINDING_MATRIX]` is structured-only), `.WithRequestProblemInformation(true)` so a refusing broker returns the `ReasonString` the `Refused` detail carries (without it the reason string is absent and every refusal dead-letters with a bare code), and `.WithCleanStart(false)` under a non-zero `.WithSessionExpiryInterval(seconds)` so in-flight QoS-1 state survives a reconnect — the ONLY resume this protocol carries, since it publishes no offset and no sequence; `MqttApplicationMessageBuilder.WithMessageExpiryInterval(seconds)` reads the envelope's own `expirytime` rostered attribute where the producing fact declared one and the row's `expiry` setting otherwise, so a broker drops an undeliverable message at its own edge on the instant the producer named and never on a window this leg invented, and every attribute crosses through `MqttUserProperty`'s `ReadOnlyMemory<byte>` `ValueBuffer` overload (the string `WithUserProperty` is `[Obsolete]`); the awaited `PublishAsync` returns `MqttClientPublishResult` whose `IsSuccess` (`MqttClientPublishReasonCode.Success` or `NoMatchingSubscribers`, a delivered-but-unrouted success and never a fault) → `Persisted`, a transport ambiguity (client disconnect, timeout) → `Indeterminate` (the held cursor re-drives and receiver-side dedup on `(source, id)` absorbs the replay), and a definitive `128`+ reason code (`NotAuthorized`, `TopicNameInvalid`, `QuotaExceeded`, `PayloadFormatInvalid`) → `Refused` with its reason string. This client never throws on the wire at all — connect, publish, subscribe, and unsubscribe each return their reason code as a VALUE and its disconnect cause is a field on `MqttClientDisconnectedEventArgs`, so `128`+ on the returned result is the whole fault vocabulary and the disconnect event is what the row's `Watch` cell reads; a leg catching exceptions here catches only the builder's construction-time `MqttProtocolViolationException`, which never crosses the boundary.
- Auto: refusal SHAPE is a column too, and it is the one every naive adapter gets wrong — `throw` alone covers redis and clickhouse, MQTT is `value` only (its publish never throws on the wire), Pulsar adds reactive `IState`, RabbitMQ and NATS and Kafka add events and callbacks, so the family reads its awaited return AND the row's `Watch` cell rather than assuming one rail every engine shares; the `http` row's multi-row drain encodes ONE `application/cloudevents-batch` body through `CloudEventFormatter.EncodeBatchModeMessage(IEnumerable<CloudEvent>, out ContentType)` only under a `WebhookSettle.PerEnvelope` receiving contract that also advertises the batch media type (`MimeUtilities.IsCloudEventsBatchContentType`), never on the transport's own say-so — `net.http_post` hands back one `bigint` id, `net._http_response` stores ONE `status_code` against that id, and the CloudEvents batch binding defines no per-event response element, so a receiver settling per REQUEST answers N envelopes with a single status and the drain reports one merged tally where `EgressReceipt` carries `Delivered` beside `Duplicates` as separate halves, a tally that cannot tell zero redelivery from a wedged retry and states a duplicate count no response ever produced; `PerRequest` is therefore the pg_net floor and its cursor-advancing drain posts SINGLE-row bodies, while `PerEnvelope` names a receiver publishing a per-envelope disposition its `Read` arrow folds into one `DeliveryAck` per envelope in offer order — which is what makes a batched round trip honest rather than merely cheap; a body past the row's own `dataref` threshold never reaches either contract, because the payload externalizes at the residence port and the envelope ships the reference, dedup honesty is a COLUMN, not prose — every row states what absorbs a replay: NATS the broker dedup window on `Nats-Msg-Id`, Kafka the idempotent producer and consumer-side `(source, id)` dedup, http/pulsar/wire-native/amqp/mqtt receiver-side dedup on that same composite, redis the producer-side `StreamIdempotentId`, clickhouse the producer-side `insert_deduplication_token`; content mode is the binding row's own `Modes` column and the subscription's `protocolsettings` selection within it, never a per-leg literal — every header-bearing transport reaches binary so a broker filters on the prefixed attribute names without parsing the body, and each row's prefix (`ce-` HTTP, `ce_` Kafka, `cloudEvents_` AMQP, UNPREFIXED MQTT) is a column rather than a spelling each leg repeats; serdes-governed Kafka bodies own the `Data` bytes and their schema-id framing beside the `ce_*` envelope headers with zero key collision — the composition root's payload arrow frames them through `AvroSerializer<T>`/`JsonSerializer<T>`/`ProtobufSerializer<T>` `SerializeAsync` over one `CachedSchemaRegistryClient` under `SchemaRegistryConfig`, so registry framing precedes the envelope and envelope codec and body codec never share a `JsonSerializerOptions`.
- Receipt: per-subscription delivery evidence rides the drain receipt (`#EGRESS_PUMP`); a subscription names its cursor row through `SinkBinding.Key` and its transport through the `Binding` row, never a free string.
- Packages: Rasm (`Rasm.Domain` `EventEnvelope.Mint`/`.Encode`, `EventMint`, `EventType`/`EventSource`/`EventKey`, `EventExtension`/`EventRoster`, `EventFormat`/`EventFrame`, `DataGrade`, `TraceCarrier` — the branch envelope owner every leg composes), CloudNative.CloudEvents (`CloudEvent` the value crossing every leg signature, `CloudEventFormatter.EncodeBatchModeMessage` + `MimeUtilities.IsCloudEventsBatchContentType` — the batched HTTP body over the owner's own format row, +`.Kafka` `ToKafkaMessage`, +`.Amqp` `AmqpExtensions.ToAmqpMessageWithUnderscorePrefix`/`ToCloudEvent` over the AMQPNetLite `Amqp.Message` carrier — the `AMQP 1.0` leg, protocol-disjoint from `RabbitMQ.Client`), Pidgin (`Parser<char, T>`, `Parser.Rec`/`Try`/`Labelled`, `Expression.ExpressionParser.Build` + the `Operator` precedence rows, `Result.Match` — the CESQL grammar, `#SUBSCRIPTION_FILTER`), NATS.Net (`INatsJSContext.Publish`/`TryPublishAsync`/`PublishConcurrentAsync` + `NatsJSPublishConcurrentFuture.GetResponseAsync`, `NatsHeaders`, `NatsResult<T>.Error`, `PubAckResponse.Duplicate`/`.Error`), Confluent.Kafka (`ProduceAsync`, `DeliveryResult.Status`, `ProducerConfig.EnableIdempotence` + `Acks.All`, `ProduceException`/`Error.IsFatal`, `IProducer.Flush(TimeSpan)`, `InitTransactions`/`BeginTransaction`/`CommitTransaction`), Confluent.SchemaRegistry (`CachedSchemaRegistryClient`/`SchemaRegistryConfig`) + serdes (`AvroSerializer<T>`/`JsonSerializer<T>`/`ProtobufSerializer<T>` `SerializeAsync` — the composition-root payload framing), OpenTelemetry.Instrumentation.ConfluentKafka (`AsInstrumentedProducerBuilder` + `ConfluentKafkaInstrumentedProducerBuilderOptions` at the leg's builder seam; `AddKafkaProducerInstrumentation` registers on the tracer and meter builders at the AppHost root), RabbitMQ.Client (`CreateChannelOptions` confirms, `BasicPublishAsync(exchange, routingKey, mandatory, properties, body, token)`, `BasicProperties.DeliveryMode`/`DeliveryModes.Persistent`, `BasicReturnEventArgs`), MQTTnet (`MqttClientFactory.CreateMqttClient` per-instance mint, `IMqttClient.Publish` → `MqttClientPublishResult`, `MqttApplicationMessageBuilder.WithContentType`/`.WithUserProperty`/`.WithMessageExpiryInterval`, `MqttClientOptionsBuilder.WithProtocolVersion(V500)`/`.WithRequestProblemInformation`/`.WithCleanStart`/`.WithSessionExpiryInterval`, `MqttUserProperty.ValueBuffer` — the branch-owned MQTT 5.0 binding's carrier, its QoS-1 PUBACK reason-code leg, and its unprefixed v5 User Property attribute map), DotPulsar (`ISend.Send` → `MessageId`, `MessageMetadata.SequenceId`/`.Key`/`.OrderingKey`, `ProducerAccessMode.WaitForExclusive`, `ProducerState`/`IState`, `Schema.ByteArray`), StackExchange.Redis (`StreamAdd`/`StreamIdempotentId`/`StreamTrimMode.Acknowledged`), AMQPNetLite.Core (`Amqp.SenderLink.SendAsync(Message, TimeSpan)` — the awaited send whose ack IS the settlement; `Amqp.Message`; `Amqp.AmqpException.Error`; `Amqp.AmqpObject.Closed`/`AddClosedCallback` — the link-and-connection fault the `Watch` cell reads; `Amqp.Session`/`Amqp.Connection` — the link's owning pair), System.Threading.Channels (`Channel.CreateBounded<T>(BoundedChannelOptions)`, `BoundedChannelOptions(int)` under `BoundedChannelFullMode.Wait`, `ChannelWriter<T>.WriteAsync`, `ChannelReader<T>.ReadAllAsync` — the AMQP leg's own in-flight window, since that client bounds nothing), ClickHouse.Driver (`ClickHouseClient.InsertBinaryAsync` + `InsertOptions`/`QueryOptions` custom settings — the warehouse leg under the `insert_deduplication_token` producer-supplied dedup setting; `ClickHouseClient.PingAsync` — the cadence probe feeding this row's `Watch` cell, the one driver here publishing no event), pg_net (`net.http_post`/`net.request_status`/`net.http_response_result` over raw Npgsql), Google.Protobuf (`WriteLengthPrefixedTo` — the wire-native payload), BCL inbox (`System.Net.Mime.ContentType` — the framing a binding stamps, `System.Collections.Frozen` — the binding rosters).
- Growth: a new delivery target is ONE `Subscription` VALUE over an existing `Binding` row — the pump, the mint, the cursor, and the receipt are untouched and no type is added; a new transport is one `Binding` row carrying its modes, prefix, routing member, settings roster, `dataref` policy, pushdown verdict, and degrade; a new envelope attribute is one `EventExtension` row at the branch owner; a new receiving contract is one `WebhookSettle` case and a new in-flight ceiling one `protocolsettings` value; zero new surface — a per-sink envelope shape, a hand-built prefixed header, a raw provider ack crossing into the pump, a second formatter, a fire-and-forget publish on a durable row, an unawaited AMQP callback send, a batched body under a per-request contract, or a connection-state read standing in for a `Watch` cell is the deleted form.
- Boundary: the envelope is the single cross-consumer, cross-language vocabulary — the AppHost outbox relay and the durable-orchestration dispatch drain the SAME projection as their hop payload, so a per-consumer re-pack is the drift defect; `id` is the OPERATION identity and `subject` the content key, so replay dedup reads `(source, id)` and a broker sequence keys nothing; the `http` row NEVER fire-and-forgets — `net.http_post` enqueues and the response reconciliation is the only advance authority; a payload past the row's `dataref` threshold externalizes to `Store/blobstore#OBJECT_STORE` and the envelope carries the reference, so no leg holds a multi-megabyte body to encode and no streaming encoder exists beside the owner's one encode; the wire-native row reads the AppHost delivery-honesty policy (the database is excluded from the AppHost hop law; sink delivery is not); the redis row's `StreamTrimMode.Acknowledged` trim keeps the stream bounded by consumption, never a time guess; the family is egress-only — the inbound Kafka consume leg is the `Version/ingress` `CdcIngress` owner where the consumer-side instrumented twins bind, never a binding row here, and its `(source, id)` dedup is the consumer half every dedup-honesty row presumes.

```csharp signature
// --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
using System.Net.Mime;
using CloudNative.CloudEvents;
using Rasm.Domain;                                // the branch envelope owner (Domain/event) + Op, the operation key every fallible kernel threads

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

// Per-binding externalization policy. A threshold fixed estate-wide either strands the smallest transport or
// wastes the largest, so it derives from the transport's OWN negotiated limit; `Retain` declares a class and
// never a window, so the producing folder's standing obligation reaches the wire unchanged; `Dual` gates
// reference-ALONE shipping, since the specification carries no capability negotiation and a peer that cannot
// resolve a reference has no way to say so. `Residence` is not a column here — it binds once at the composition
// root as the `Store/blobstore#OBJECT_STORE` port, and an unbound port refuses at admission rather than shipping
// a reference nothing resolves.
public readonly record struct DatarefRow(int Threshold, RetentionClass Retain, bool Dual);

// ONE transport roster. Every column is a fact the SPECIFICATION or the engine fixes, so a subscription selects
// a row and supplies values — never a per-sink knob re-deciding what the transport already answers. `Spec` marks
// the rows a CloudEvents protocol binding backs; a false row is a house envelope leg carrying the same envelope
// over a property map of its own, which is an honest column rather than a silent second vocabulary.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class Binding {
    // Prefixes differ per binding and a leg reusing a neighbour's publishes names no conforming peer resolves:
    // `ce-` is HTTP alone, `ce_` Kafka alone, `cloudEvents_` AMQP alone, and MQTT 5.0 carries NO prefix at all.
    public static readonly Binding Http = new("http",
        modes: [ContentMode.Binary, ContentMode.Structured], prefix: "ce-", batches: true, pushdown: false,
        routesOn: None, required: ["endpoint"], optional: ["method", "headers"],
        dataref: new(Threshold: 8 << 10, RetentionClass.Blob, Dual: false), spec: true,
        degrade: "one status per request, and one PENDING holds the cursor for a whole drain");

    public static readonly Binding Kafka = new("kafka",
        modes: [ContentMode.Binary, ContentMode.Structured], prefix: "ce_", batches: true, pushdown: false,
        routesOn: Some(EventExtension.PartitionKey), required: ["topicname"], optional: ["partitionkeyextractor", "clientid", "acks", "mode"],
        dataref: new(Threshold: 1 << 20, RetentionClass.Blob, Dual: false), spec: true,
        degrade: "`Error.IsRetriable` is internal; transactions never span the cursor");

    // The one UNPREFIXED row: v5 User Properties carry the bare attribute names, so a `ce_` or `ce-` spelling
    // here is a conformance defect a peer silently drops rather than a stylistic choice.
    public static readonly Binding Mqtt = new("mqtt",
        modes: [ContentMode.Binary, ContentMode.Structured], prefix: "", batches: false, pushdown: true,
        routesOn: None, required: ["topicname"], optional: ["qos", "retain", "expiry", "userproperties"],
        dataref: new(Threshold: 256 << 10, RetentionClass.Blob, Dual: true), spec: true,
        degrade: "no key and no origin; every v5 field drops silently under `V311`, which is structured-only");

    public static readonly Binding Amqp = new("amqp",
        modes: [ContentMode.Binary, ContentMode.Structured], prefix: "cloudEvents_", batches: false, pushdown: true,
        routesOn: None, required: ["address"], optional: ["linkname", "sendersettlementmode", "linkproperties", "inflight"],
        dataref: new(Threshold: 512 << 10, RetentionClass.Blob, Dual: false), spec: true,
        degrade: "no sender credit member at all; this fence bounds its own in-flight");

    public static readonly Binding Nats = new("nats",
        modes: [ContentMode.Binary, ContentMode.Structured], prefix: "ce-", batches: false, pushdown: true,
        routesOn: None, required: ["subject"], optional: ["stream"],
        dataref: new(Threshold: 1 << 20, RetentionClass.Blob, Dual: false), spec: true,
        degrade: "no key member, so per-entity order needs one subject per entity");

    public static readonly Binding RabbitMq = new("rabbitmq",
        modes: [ContentMode.Binary, ContentMode.Structured], prefix: "ce-", batches: false, pushdown: false,
        routesOn: None, required: ["exchange", "routingkey"], optional: ["expiration"],
        dataref: new(Threshold: 1 << 20, RetentionClass.Blob, Dual: false), spec: true,
        degrade: "auto-recovery swallows a drop the caller never observes");

    // House legs: the same envelope over a property map the specification never defined, so `Spec` is false and
    // `Prefix` is empty because there is no prefixed attribute contract to honour or to violate.
    public static readonly Binding Pulsar = new("pulsar",
        modes: [ContentMode.Structured], prefix: "", batches: false, pushdown: false,
        routesOn: Some(EventExtension.PartitionKey), required: ["topic"], optional: ["accessmode"],
        dataref: new(Threshold: 5 << 20, RetentionClass.Blob, Dual: false), spec: false,
        degrade: "no transactions; a fenced producer surfaces only on `IState`");

    public static readonly Binding Redis = new("redis",
        modes: [ContentMode.Structured], prefix: "", batches: false, pushdown: false,
        routesOn: None, required: ["stream", "group"], optional: [],
        dataref: new(Threshold: 512 << 10, RetentionClass.Blob, Dual: false), spec: false,
        degrade: "the consumer group's cursor never governs the outbox cursor");

    public static readonly Binding ClickHouse = new("clickhouse",
        modes: [ContentMode.Structured], prefix: "", batches: true, pushdown: false,
        routesOn: None, required: ["table"], optional: [],
        dataref: new(Threshold: 4 << 20, RetentionClass.Blob, Dual: false), spec: false,
        degrade: "no events at all; `BeginDbTransaction` throws, the token is the dedup story");

    public static readonly Binding WireNative = new("wirenative",
        modes: [ContentMode.Structured], prefix: "", batches: false, pushdown: false,
        routesOn: None, required: ["hopkey"], optional: [],
        dataref: new(Threshold: 4 << 20, RetentionClass.Blob, Dual: false), spec: false,
        degrade: "no broker behind the hop, so an undelivered envelope rests in the letter table");

    private Binding(string key, FrozenSet<ContentMode> modes, string prefix, bool batches, bool pushdown,
        Option<EventExtension> routesOn, FrozenSet<string> required, FrozenSet<string> optional,
        DatarefRow dataref, bool spec, string degrade) : this(key) =>
        (Modes, Prefix, Batches, Pushdown, RoutesOn, Required, Optional, Dataref, Spec, Degrade) =
        (modes, prefix, batches, pushdown, routesOn, required, optional, dataref, spec, degrade);

    public FrozenSet<ContentMode> Modes { get; }
    public string Prefix { get; }
    public bool Batches { get; }
    // Whether the BROKER resolves a routing filter — MQTT on SUBSCRIBE, AMQP on a link-source filter, NATS on a
    // subject wildcard. Kafka and HTTP have no server-side mechanism at all, so their filters run consumer-side
    // and a `prefix`/`exact` dialect on those rows costs a delivered-then-discarded message rather than nothing.
    public bool Pushdown { get; }
    public Option<EventExtension> RoutesOn { get; }
    public FrozenSet<string> Required { get; }
    public FrozenSet<string> Optional { get; }
    public DatarefRow Dataref { get; }
    public bool Spec { get; }
    public string Degrade { get; }

    // Derived membership, never a second roster: a caller asking which transports resolve a filter at the broker
    // reads the rows' own column, so a new row joins both answers with no edit here.
    public static readonly Lazy<FrozenSet<Binding>> BrokerFiltered =
        new(static () => Items.Where(static row => row.Pushdown).ToFrozenSet());
}

// --- [MODELS] ---------------------------------------------------------------------------

// The specification's `protocolsettings` slice, admitted ONCE against the binding row's own roster. Unknown keys
// FAIL CLOSED and missing required keys refuse, so a settings map is proven readable before a subscription
// exists — a key accepted and silently ignored is worse than a rejected one, since it publishes a governance
// value no leg ever consumes. Values stay text because the wire carries text; each leg parses its own.
public sealed record ProtocolSettings {
    private ProtocolSettings(Map<string, string> values) => Values = values;

    public Map<string, string> Values { get; }

    public static Fin<ProtocolSettings> Admit(Binding binding, Map<string, string> settings, Op key) =>
        toSeq(binding.Required).Filter(name => settings.Find(name).IsNone) is { IsEmpty: false } absent
            ? Fin.Fail<ProtocolSettings>(new EgressFault.SettingsRejected(binding.Key, $"<absent:{string.Join(',', absent)}>", Some(key)))
            : toSeq(settings.Keys).Filter(name => !binding.Required.Contains(name) && !binding.Optional.Contains(name)) is { IsEmpty: false } unknown
                ? Fin.Fail<ProtocolSettings>(new EgressFault.SettingsRejected(binding.Key, $"<unknown:{string.Join(',', unknown)}>", Some(key)))
                : Fin.Succ(new ProtocolSettings(settings));

    // Required keys read TOTAL by construction — admission already proved presence, so the interior never
    // re-tests and never carries a null branch for a value the boundary settled.
    public string this[string name] => Values[name];

    public Option<string> Optional(string name) => Values.Find(name);
}

// ONE ack family every provider outcome folds to at its own boundary — a raw PubAckResponse /
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

// `SinkBinding` is the shared subscription row data: the cursor-row key, the drain batch width, the held
// fencing token the cursor advance validates, the bound delivery leg, and the out-of-band fault reader — the
// composition root fills `Leg` from the provider client (the blobstore `GrantMinter` idiom), so provider SDK
// types never enter this fence.
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
    Func<Subscription, CloudEvent, OpLogEntry, IO<DeliveryAck>> Leg,
    Func<Option<string>> Watch);

// A delivery target is a VALUE: one binding row, its admitted settings, its filter AND-set, and its cursor row.
// Ten transports that were ten union cases are ten values a deployment authors, so a second subscription over one
// transport (a second topic, a narrower filter, a different tenant) costs one row in a table rather than a case
// in a compiler-held family — which is exactly what a subscription resource is for.
public sealed record Subscription(SinkBinding Bind, Binding Binding, ProtocolSettings Settings, Seq<FilterDialect> Filters) {
    // Watch reads the row's out-of-band cell AFTER the leg settles and downgrades a Persisted alone: an
    // acknowledged send standing beside a pending transport fault is exactly the ambiguous case, so the cursor
    // HOLDS and the row re-drives into the binding's own dedup rather than advancing past a row the transport
    // reported lost on a surface the await never saw. Indeterminate and Refused already carry their own detail
    // and take no second one; a clean cell is the common path and costs one Option read.
    public IO<DeliveryAck> Deliver(CloudEvent envelope, OpLogEntry row) =>
        Bind.Leg(this, envelope, row).Map(ack => (ack, Bind.Watch()) switch {
            (DeliveryAck.Persisted, { IsSome: true, Case: string pending }) => new DeliveryAck.Indeterminate(pending),
            _                                                              => ack,
        });

    // `filters` is an AND-set: any false expression withholds delivery, and an EMPTY set delivers, because the
    // identity of conjunction is true and an unfiltered subscription is the common case. Evaluation is TOTAL —
    // every dialect answers a verdict and an accumulated fault list — so a CESQL runtime error withholds the
    // event without darkening the subscription, and the faults ride the drain's own observe point.
    public FilterVerdict Matches(CloudEvent envelope) =>
        Filters.Fold(FilterVerdict.Pass, (held, dialect) => held.And(dialect.Evaluate(envelope)));
}

// --- [OPERATIONS] -----------------------------------------------------------------------

public static class Egress {
    // The capability subject every `rasm.*` metric name and every envelope `type` this folder emits shares, so a
    // board and a subscription join ONE vocabulary and a rename moves both at once.
    public const string Domain = "persistence";

    // `Envelope` is the ONE mint: it composes the branch owner, so `Validate()` runs on every projected row and
    // a malformed grammar value refuses HERE instead of reaching a transport that would take it. `id` carries the
    // OPERATION identity the dot already owns — a content digest there makes two peers stamping identical bytes
    // into one event and drops the second — while the content key rides `subject` under the owner's ONE `EventKey`
    // spelling. `datacontenttype` and `dataschema` are ROW DATA off the payload arrow that framed the body, so a
    // registry-framed Avro or Protobuf body announces what it is rather than an unconditional octet-stream every
    // consumer then decodes by convention. The trace stamped is the CREATION-time pair the entry persisted,
    // rendered by the propagator that owns the format; the current hop rides each binding's own headers.
    public static IO<Fin<CloudEvent>> Envelope(OpLogEntry row, Subscription sink, EgressPorts ports) =>
        Resided(row, sink, ports).Map(framed => Minted(row, sink, ports, framed));

    // Externalization is an EFFECT and it lands before the envelope exists, because a `dataref` naming bytes no
    // store holds is a reference the receiver resolves to nothing. A body inside the row's own threshold never
    // reaches the port at all, so the common path stays one pure projection over the framed payload and the
    // residence write happens exactly where the transport's negotiated limit forces it.
    static IO<PayloadFrame> Resided(OpLogEntry row, Subscription sink, EgressPorts ports) =>
        ports.Frame(row) switch {
            var framed when framed.Body.Length <= sink.Binding.Dataref.Threshold => IO.pure(framed),
            var framed => ports.Reside(framed).Map(key => framed with { Residence = Some(key) }),
        };

    static Fin<CloudEvent> Minted(OpLogEntry row, Subscription sink, EgressPorts ports, PayloadFrame framed) =>
        EventEnvelope.Mint(
            new EventMint(
                Type: EventType.Of(Domain, subject: row.Family.Key, fact: row.Kind.Fact, major: framed.Major),
                Source: EventSource.Of(Domain, capability: "oplog"),
                Id: row.Id.Wire,
                Subject: Some(EventKey.Render(row.ContentKey)),
                Time: row.Physical,
                DataSchema: framed.Schema,
                DataContentType: Some(framed.ContentType),
                Data: Carried(framed, sink),
                Trace: row.Trace.Continue().Map(ports.Carrier).IfNone(default(TraceCarrier)),
                Extensions: Rows(row, sink, framed)),
            Op.Of(name: nameof(Egress)));

    // The payload SHIPS only where the reference alone cannot serve: `Dual` names a peer holding no residence
    // credential, so the reference rides BESIDE the body it describes; every other externalized row replaces the
    // body with it, which is the whole reason a threshold the transport's own limit fixed exists. Shipping both on
    // every row would make the threshold a decoration that grows the message it was written to shrink.
    static object? Carried(PayloadFrame framed, Subscription sink) =>
        framed.Residence.IsNone || sink.Binding.Dataref.Dual ? framed.Body : null;

    // Rostered writes as DATA: every dimension is one `EventExtension` row the branch owner declared, so a new
    // one lands as a row above and one entry here. `RoutesOn` is what makes the routing member the BINDING's
    // answer — the two rows exposing a real key member get the entity key, and the rest get none rather than an
    // attribute their engine cannot route on.
    static Seq<(EventExtension Row, object Value)> Rows(OpLogEntry row, Subscription sink, PayloadFrame framed) =>
        Seq((EventExtension.DataClassification, (object)framed.Grade.Key),
            (EventExtension.Sequence, row.Sequence.ToString(CultureInfo.InvariantCulture)),
            (EventExtension.SequenceType, "Integer"))
        + sink.Binding.RoutesOn.Map(partition => Seq((partition, (object)row.EntityKey))).IfNone(Seq<(EventExtension, object)>())
        + Externalized(framed);

    // The `dataref` decision is the BINDING's, because the threshold is that transport's own negotiated limit: a
    // body the row cannot carry crosses the residence port and the envelope publishes the key that port answered,
    // which is the address a receiver dials. `subject` and `dataref` share ONE spelling and name two different
    // things — the durable row's own content key and the residence address of the redacted, framed bytes — so a
    // reader joins on either without a second rendering, and a fence deriving the reference by re-hashing the body
    // publishes an address the store was never asked to hold.
    static Seq<(EventExtension Row, object Value)> Externalized(PayloadFrame framed) =>
        framed.Residence
            .Map(static key => Seq<(EventExtension Row, object Value)>((EventExtension.DataRef, new Uri(EventKey.Render(key), UriKind.Relative))))
            .IfNone(Seq<(EventExtension Row, object Value)>());
}
```

Selection descriptor — the sentence a row is chosen on, the member a message enters through, the mechanism realizing the closed `none | single | multi` tenancy axis, and who ends a message's life. `[DEGRADE]` is the `Binding` row's own column and publishes there alone, so no table restates what the roster already answers.

| [INDEX] | [BINDING]    | [FITS]                          | [ADMIT]                  | [TENANCY]                  | [LIFETIME]                    |
| :-----: | :----------- | :------------------------------ | :----------------------- | :------------------------- | :---------------------------- |
|   [01]  | `http`       | one HTTP consumer, no broker    | `net.http_post` enqueue  | per-tenant target `Uri`    | letter table; no server hold  |
|   [02]  | `nats`       | low-latency dedup-window fan    | `INatsJSContext.Publish` | account or subject prefix  | `StreamConfig` age/msgs/bytes |
|   [03]  | `kafka`      | high-volume partition log       | `ProduceAsync`           | topic prefix under ACL     | broker topic retention        |
|   [04]  | `rabbitmq`   | routed work queue with confirms | `BasicPublishAsync`      | NATIVE vhost               | `Expiration` + queue TTL      |
|   [05]  | `amqp`       | header-routed `AMQP 1.0` peer   | `SenderLink.SendAsync`   | address prefix             | broker-side, no member        |
|   [06]  | `pulsar`     | geo-replicated tiered log       | `ISend.Send`             | NATIVE `tenant/namespace`  | namespace retention           |
|   [07]  | `wirenative` | in-estate gRPC peer             | `OutboundHop` pipeline   | `TenantContext` on the hop | hop deadline; no persistence  |
|   [08]  | `redis`      | consumer-group work stream      | `StreamAdd`              | key prefix                 | `StreamTrimMode.Acknowledged` |
|   [09]  | `clickhouse` | billion-row analytics ingest    | `InsertBinaryAsync`      | tenant-led sort key        | table TTL                     |
|   [10]  | `mqtt`       | constrained sensor/edge peer    | `IMqttClient.Publish`    | topic prefix               | `WithMessageExpiryInterval`   |

Guarantee coordinates every engine DECIDES for itself, and the differences ARE the point: one value repeating across engines that genuinely differ is a row that stopped reading its engine.

| [INDEX] | [BINDING]    | [DELIVER]                          | [ORDER]                      | [SETTLE]                             |
| :-----: | :----------- | :--------------------------------- | :--------------------------- | :----------------------------------- |
|   [01]  | `http`       | at-least-once                      | none                         | reconciled `request_status=SUCCESS`  |
|   [02]  | `nats`       | at-least-once + dedup window       | subject; NO key member       | `PubAckResponse`, two error surfaces |
|   [03]  | `kafka`      | at-least-once, idempotent producer | partition by `Message.Key`   | 3-valued `PersistenceStatus`         |
|   [04]  | `rabbitmq`   | at-least-once, publisher confirms  | queue by routing key; NO key | confirm completion; nack THROWS      |
|   [05]  | `amqp`       | at-least-once                      | link/address; NO key member  | awaited `SendAsync`; refusal THROWS  |
|   [06]  | `pulsar`     | at-least-once + fenced leader      | partition + `OrderingKey`    | `MessageId` alone; NO status enum    |
|   [07]  | `wirenative` | exactly-once-effective             | none                         | `OutboundHop` honesty verdict        |
|   [08]  | `redis`      | at-least-once                      | stream                       | returned stream id                   |
|   [09]  | `clickhouse` | at-least-once                      | none; insert order is no key | awaited insert completion            |
|   [10]  | `mqtt`       | QoS-1 at-least-once                | topic; NO key member         | PUBACK reason code, never a throw    |

Recovery coordinates — where a re-drive resumes, what bounds in-flight work, and the SHAPE a refusal arrives in, which is the rail trap the `SinkBinding.Watch` cell closes. No row carries a retry schedule: the held cursor owns re-drive for the whole family, and each row's own client or hop owns retry and breaker beneath it.

| [INDEX] | [BINDING]    | [REPLAY]                          | [BOUND]                          | [REFUSE]                 |
| :-----: | :----------- | :-------------------------------- | :------------------------------- | :----------------------- |
|   [01]  | `http`       | no origin; receiver `(source,id)` | pg_net queue + sliding window    | value                    |
|   [02]  | `nats`       | `DeliverPolicy`, `GetDirectAsync` | `MaxAckPending`, bounded channel | value + throw + event    |
|   [03]  | `kafka`      | `Seek`/`Offset`/`OffsetsForTimes` | `Flush`/`Poll`/`Pause`           | throw + value + callback |
|   [04]  | `rabbitmq`   | NONE — head of queue only         | `BasicQos` + confirms limiter    | throw + event            |
|   [05]  | `amqp`       | NONE; receiver `(source,id)`      | the `inflight` settings window   | throw + event            |
|   [06]  | `pulsar`     | `MessageId` seek, cursorless read | pending cap + prefetch           | throw + reactive state   |
|   [07]  | `wirenative` | receiver `(source,id)`            | hop admission row                | throw (`RpcException`)   |
|   [08]  | `redis`      | `StreamIdempotentId`              | trim by acknowledgement          | throw                    |
|   [09]  | `clickhouse` | `insert_deduplication_token`      | pooled insert                    | throw                    |
|   [10]  | `mqtt`       | session state only; `(source,id)` | NONE — the caller bounds it      | value only               |

## [04]-[SUBSCRIPTION_FILTER]

- Owner: `FilterDialect` the closed seven-dialect predicate family a `Subscription` carries as an AND-set; `FilterVerdict` the total answer every dialect returns — a delivery bit beside the faults its evaluation accumulated; `Cesql` the table-driven expression owner over the `sql` dialect, holding the one built-once grammar, the three-type value family, the function table, the implicit-cast matrix, and the accumulating evaluator; `CesqlFault` the seven specification error types on the 8530 band.
- Cases: `exact` case-sensitive equality, `prefix`/`suffix` the two affix tests, `all`/`any` the recursive conjunction and disjunction, `not` the recursive negation, `sql` a CESQL expression. The specification makes `sql` OPTIONAL and this fabric makes it mandatory, because a subscription that can express only attribute affixes pushes every real routing decision back into a consumer that must decode the payload to make it.
- Entry: `FilterDialect.Evaluate(envelope)` answers a `FilterVerdict`; `Cesql.Compile(text, key)` parses ONCE at subscription admission and rails an unparseable expression as `EgressFault.SettingsRejected` — never a delivery; `CesqlExpression.Evaluate(envelope)` runs the compiled tree per event.
- Auto: evaluation is TOTAL — every operator, function, and cast answers a value and an accumulated fault list, so a runtime error withholds one event rather than darkening a subscription, and a subscription whose expression names a missing attribute keeps routing every event whose attribute IS present. The AND-set's identity is delivery, so a subscription carrying no filter delivers; `not` inverts the bit and PRESERVES the faults, because a fault under a negation is still a fault the operator did not observe.
- Auto: `Integer` is 32-bit and the proof is `ABS(-2147483648)` — the negation of the minimum has no 32-bit representation, so the evaluator answers a `MathError` beside a defined value where an unchecked `Math.Abs` throws under this branch's `CheckForOverflowUnderflow`; every arithmetic arm reads the same guard, so overflow is a fault row rather than an escaping exception.
- Auto: the grammar is a PRECEDENCE TABLE folded through `ExpressionParser.Build`, never a recursive-descent ladder over mutable state — one ordered row set per precedence level, `Rec` at the one self-reference the parenthesised sub-expression needs, `Try` at every alternation whose branches share a prefix, and `Labelled` on every terminal so a refusal reports the grammar's own vocabulary rather than a character class.
- Law: pushdown is the `Binding` row's own `Pushdown` column and never a dialect property — MQTT resolves a topic filter at the broker on SUBSCRIBE, AMQP through a link-source filter, NATS through a subject wildcard, while Kafka has no server-side filtering and HTTP no native mechanism, so an `exact` or `prefix` dialect on those two rows costs a delivered-then-discarded message. `sql` is consumer-side on every row, since no broker in the roster evaluates it.
- Law: a compiled expression is a VALUE built once and held for the subscription's life — parsers are immutable values and a grammar constructed per evaluation rebuilds the whole expression graph on every event, which is the difference between a per-event allocation and none.
- Receipt: accumulated faults ride the drain's own `rasm.persistence.egress.delivered` observe point beside the filtered count (`#EGRESS_PUMP`), so an expression quietly erroring on every event is visible as a rate rather than as silence.
- Packages: Pidgin (`Parser<char, T>`, `Parser.String`/`Char`/`Num`/`OneOf`/`Try`/`Rec`/`Map`, `parser.Or`/`.Many`/`.Separated`/`.Optional`/`.Labelled`/`.Between`, `Expression.ExpressionParser.Build`, `Expression.Operator.InfixL`/`InfixN`/`Prefix`, `Expression.OperatorTableRow.And`, `parser.Parse` + `Result.Match` — the fold onto this branch's own rail), Rasm (`Rasm.Domain` `Expected`, `EventRoster.Resolve` — the declared attribute set an expression names), CloudNative.CloudEvents (`CloudEvent.GetAttribute`, `CloudEventAttribute.Format` — the one rendering an expression compares against), Thinktecture.Runtime.Extensions (`[Union]`/`[Union<T1,T2,T3>]`/`[SmartEnum<string>]`), LanguageExt.Core (`Fin`/`Seq`/`Map`), BCL inbox (`System.Globalization`).
- Growth: a new dialect is one `FilterDialect` case and one `Evaluate` arm; a new CESQL function is one `CesqlFunction` row carrying its arity and its total body, and the parser, the cast matrix, and the evaluator read it untouched; a new operator is one precedence-table row.
- Boundary: filters decide DELIVERY and never mutate an envelope, so an expression is a pure read over admitted attributes; the attribute vocabulary an expression may name is the branch owner's declared roster, so an unrostered name answers `MissingAttributeError` rather than reaching an untyped string a producer happened to set; subscription persistence, the management API, and the `protocolsettings` roster seat at `#EGRESS_SINK`.

| [INDEX] | [DIALECT] | [SHAPE]                                        | [PUSHDOWN]                                     |
| :-----: | :-------- | :--------------------------------------------- | :--------------------------------------------- |
|  [01]   | `exact`   | attribute → value, case-sensitive equality     | broker rows, on the routing attribute          |
|  [02]   | `prefix`  | attribute → value `startsWith`                 | broker rows where the attribute IS the route   |
|  [03]   | `suffix`  | attribute → value `endsWith`                   | consumer-side on every row                     |
|  [04]   | `all`     | recursive conjunction over nested dialects     | only where every child pushes down             |
|  [05]   | `any`     | recursive disjunction over nested dialects     | only where every child pushes down             |
|  [06]   | `not`     | recursive negation                             | consumer-side on every row                     |
|  [07]   | `sql`     | a compiled CESQL expression                    | consumer-side always                           |

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

// The specification's three value spaces. Ad-hoc union rather than three wrapper records: the slot IS the type
// tag the cast matrix reads, and `Integer` is 32-bit by declaration — the one fact `ABS(-2147483648)` proves,
// since a 64-bit reading would answer that call cleanly and never reach the `MathError` arm. `ConversionFromValue`
// keeps its generated implicit default, which is what lets every operator body, function row, and literal arm hand
// its own `string`, `int`, or `bool` straight into the carrier — suppressing it forces a wrapper spelling at each
// of those sites for no discriminant the slot does not already carry.
[Union<string, int, bool>(T1Name = "Text", T2Name = "Number", T3Name = "Flag")]
public readonly partial struct CesqlValue;

// Evaluation is TOTAL, so the answer is a value AND its accumulated faults — never a rail choosing one. A
// `Validation` here would force the evaluator to abandon a value the specification requires it to produce, and a
// bare value would discard the diagnostics an operator raised on the way to it.
public readonly record struct CesqlResult(CesqlValue Value, Seq<CesqlFault> Faults) {
    public static CesqlResult Of(CesqlValue value) => new(value, Seq<CesqlFault>());

    public CesqlResult Fault(CesqlFault fault) => this with { Faults = Faults.Add(fault) };

    // Faults union across operands, so a binary node reports both sides' diagnostics rather than the first it
    // happened to evaluate — the accumulation the specification's own error list describes.
    public CesqlResult Join(CesqlResult other, CesqlValue value) => new(value, Faults + other.Faults);
}

// The delivery answer every dialect returns. `And` is the AND-set's fold and `Pass` its identity, so an empty
// filter set delivers; both halves carry faults forward, because a negation or a short-circuit that discards
// them hides exactly the expression that is silently failing on every event.
public readonly record struct FilterVerdict(bool Delivers, Seq<CesqlFault> Faults) {
    public static readonly FilterVerdict Pass = new(true, Seq<CesqlFault>());

    public FilterVerdict And(FilterVerdict other) => new(Delivers && other.Delivers, Faults + other.Faults);

    public FilterVerdict Or(FilterVerdict other) => new(Delivers || other.Delivers, Faults + other.Faults);

    public FilterVerdict Negate() => this with { Delivers = !Delivers };
}

// The seven dialects as one closed family: `all`/`any`/`not` NEST the root, so a nested filter tree is a shape
// property the recursion owns and every consumer's dispatch stays total; `sql` carries a COMPILED expression, so
// the grammar crossed its admission before the subscription existed and no delivery ever parses text.
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
    // text the wire carries and a `Timestamp` or `Uri` attribute matches the spelling a peer sees rather than a
    // CLR `ToString()` this fence chose. An absent attribute answers false WITHOUT a fault — the specification's
    // affix dialects are membership tests, and a missing attribute is a legitimate non-match rather than an
    // error, which is precisely the distinction `sql`'s `MissingAttributeError` draws.
    public FilterVerdict Evaluate(CloudEvent envelope) => Switch(
        state: envelope,
        exact:  static (event_, node) => Held(event_, node.Attribute, held => string.Equals(held, node.Value, StringComparison.Ordinal)),
        prefix: static (event_, node) => Held(event_, node.Attribute, held => held.StartsWith(node.Value, StringComparison.Ordinal)),
        suffix: static (event_, node) => Held(event_, node.Attribute, held => held.EndsWith(node.Value, StringComparison.Ordinal)),
        // `All` folds from Pass and `Any` from its own false identity, so an empty child set answers the
        // operator's identity rather than a fabricated verdict.
        all:    static (event_, node) => node.Children.Fold(FilterVerdict.Pass, (held, child) => held.And(child.Evaluate(event_))),
        any:    static (event_, node) => node.Children.Fold(FilterVerdict.Pass with { Delivers = false }, (held, child) => held.Or(child.Evaluate(event_))),
        not:    static (event_, node) => node.Child.Evaluate(event_).Negate(),
        sql:    static (event_, node) => Answered(node.Expression.Evaluate(event_)));

    // The compiled expression's own total answer IS the verdict: its value casts to the delivery bit and its
    // accumulated faults ride forward unchanged, so an expression erroring on every event stays visible as a rate.
    static FilterVerdict Answered(CesqlResult answered) => new(CesqlCast.Flag(answered.Value), answered.Faults);

    static FilterVerdict Held(CloudEvent envelope, string name, Func<string, bool> test) =>
        Optional(envelope.GetAttribute(name)).Bind(attribute => Optional(envelope[attribute]).Map(value => attribute.Format(value)))
            .Match(Some: held => FilterVerdict.Pass with { Delivers = test(held) },
                   None: () => FilterVerdict.Pass with { Delivers = false });
}

// --- [ERRORS] ---------------------------------------------------------------------------
// Band 8530 (`FaultBand.Cesql`): the specification's SEVEN error types, each carrying the evidence its own
// diagnosis needs. These are accumulated EVIDENCE on a total evaluation rather than rail failures — only
// `ParseError` reaches a rail, at subscription admission, where an unparseable expression refuses the
// subscription itself and never one delivery.
[Union]
public abstract partial record CesqlFault : Rasm.Domain.Expected, IValidationError<CesqlFault> {
    private CesqlFault() : base() { }

    // The one case that reaches a RAIL carries the admitting operation's key, exactly as the kernel's own value
    // faults do; the six accumulated cases ride an evaluation with no operation context to name.
    public sealed record ParseError(string Detail, Option<Op> Key = default) : CesqlFault;
    public sealed record MathError(string Operator, string Detail) : CesqlFault;
    public sealed record CastError(string From, string To) : CesqlFault;
    public sealed record MissingAttributeError(string Attribute) : CesqlFault;
    public sealed record MissingFunctionError(string Function) : CesqlFault;
    public sealed record FunctionEvaluationError(string Function, string Detail) : CesqlFault;
    public sealed record GenericError(string Detail) : CesqlFault;

    public override int Code => FaultBand.Cesql + Switch(
        parseError:              static _ => 1,
        mathError:               static _ => 2,
        castError:               static _ => 3,
        missingAttributeError:   static _ => 4,
        missingFunctionError:    static _ => 5,
        functionEvaluationError: static _ => 6,
        genericError:            static _ => 7);

    public override string Message => Switch(
        parseError:              static c => $"<cesql-parse>:{c.Detail}",
        mathError:               static c => $"<cesql-math:{c.Operator}>:{c.Detail}",
        castError:               static c => $"<cesql-cast:{c.From}->{c.To}>",
        missingAttributeError:   static c => $"<cesql-attribute:{c.Attribute}>",
        missingFunctionError:    static c => $"<cesql-function:{c.Function}>",
        functionEvaluationError: static c => $"<cesql-eval:{c.Function}>:{c.Detail}",
        genericError:            static c => $"<cesql-generic>:{c.Detail}");

    public override string Category => Switch(
        parseError:              static _ => "Parse",
        mathError:               static _ => "Math",
        castError:               static _ => "Cast",
        missingAttributeError:   static _ => "Attribute",
        missingFunctionError:    static _ => "Function",
        functionEvaluationError: static _ => "Evaluation",
        genericError:            static _ => "Generic");

    public static CesqlFault Create(string message) => new GenericError(message);
}

// --- [TABLES] ---------------------------------------------------------------------------
// The nine specification functions as ROWS, each carrying its arity and its TOTAL body — a body answering a
// value and a fault rather than raising. One table serves the parser (which names are callable), the evaluator
// (what each does), and the diagnostic (what an unknown name reports), so a tenth function is one row and no
// dispatch site moves. `Abs` is the row where the 32-bit width becomes observable.
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

    // The width proof: negating `int.MinValue` has no 32-bit representation, so this row answers the minimum
    // beside a `MathError` where an unguarded `Math.Abs` throws under this branch's checked arithmetic — the one
    // call that distinguishes a conforming 32-bit evaluator from a 64-bit one that silently succeeds.
    public static readonly CesqlFunction Abs = new("ABS", arity: 1, static args =>
        CesqlCast.Number(args[0]) is int.MinValue and var floor
            ? CesqlResult.Of(floor).Fault(new CesqlFault.MathError("ABS", "<int32-overflow>"))
            : CesqlResult.Of(Math.Abs(CesqlCast.Number(args[0]))));

    private CesqlFunction(string key, int arity, Func<Seq<CesqlValue>, CesqlResult> body) : this(key) =>
        (Arity, Body) = (arity, body);

    // Negative arity is VARIADIC, so `CONCAT` states its own shape rather than forcing a second column that
    // every fixed-arity row would answer identically.
    public int Arity { get; }

    public Func<Seq<CesqlValue>, CesqlResult> Body { get; }

    public bool Admits(int count) => Arity < 0 ? count > 0 : count == Arity;

    // The requested width CLAMPS into the text rather than refusing, because both affix rows are total by the
    // specification and a caller asking past the end is asking for the whole of it.
    static CesqlResult Slice(Seq<CesqlValue> args, bool fromStart) =>
        Sliced(CesqlCast.Text(args[0]), CesqlCast.Number(args[1]), fromStart);

    static CesqlResult Sliced(string text, int requested, bool fromStart) =>
        Math.Clamp(requested, 0, text.Length) switch {
            var width => CesqlResult.Of(fromStart ? text[..width] : text[^width..]),
        };

    // An out-of-range position is a FunctionEvaluationError beside the empty string, never a raise: the
    // specification's evaluator answers a value for every call it admits.
    static CesqlResult Substr(Seq<CesqlValue> args) =>
        Substrung(CesqlCast.Text(args[0]), CesqlCast.Number(args[1]), CesqlCast.Number(args[2]));

    static CesqlResult Substrung(string text, int at, int width) =>
        at >= 1 && width >= 0 && (at - 1) + width <= text.Length
            ? CesqlResult.Of(text.Substring(at - 1, width))
            : CesqlResult.Of(string.Empty).Fault(new CesqlFault.FunctionEvaluationError("SUBSTRING", "<range>"));
}

// --- [OPERATIONS] -----------------------------------------------------------------------

// The implicit-cast matrix, one entry per (from, to) the specification defines. A cast that cannot succeed answers
// the target type's ZERO beside a `CastError`, because the evaluator is total and a raise here would make one
// malformed attribute value darken an otherwise-matching subscription. Three NAMED crossings rather than one
// target-string switch: the target is a compile-time fact at every operand site, so a stringly dispatch would turn
// it into a runtime lookup with a fall-through arm no caller can see the shape of.
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

    // The refusal-aware crossings every operand takes: the projected value AND the fault a lossy crossing owes, so
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

// The compiled expression: a recursive class-kind union whose cases nest the root, so depth growth is absorbed
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
    // producer happened to set — and each present value renders through the attribute's OWN `Format`, so the
    // text an expression compares is the exact text the wire carries.
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

    // Arity refuses at the ROW, so a call the grammar admitted but the function cannot serve reports its own
    // name rather than raising out of a body sized for a different argument count.
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

    // Membership folds the SET onto the subject's own faults, so a set element that erred reports beside the
    // verdict; `NOT IN` inverts the bit alone and the accumulated evidence rides through untouched.
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
// Operators as ROWS carrying their own total bodies, so the precedence table names a row and the evaluator
// invokes one — never a switch over a symbol string in two places that can disagree about what `%` does.
// Arithmetic rows read the checked guard once: a `MathError` beside a defined value is the specification's
// answer, and an escaping `OverflowException` under this branch's checked arithmetic is the deleted form.
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

    // Equality is SAME-TYPE by the specification, so a `String` and an `Integer` compare through the cast
    // matrix's text projection rather than answering false on a type mismatch a producer never intended.
    static bool Same(CesqlValue left, CesqlValue right) =>
        left.IsNumber && right.IsNumber
            ? left.AsNumber == right.AsNumber
            : string.Equals(CesqlCast.Text(left), CesqlCast.Text(right), StringComparison.Ordinal);

    // ONE admission crossing every typed arm takes: the operand's own faults plus whatever its cast owed, carried on
    // the operand's own value, so a lossy text-to-number or text-to-boolean read reports its `CastError` rather
    // than comparing against a silent zero the specification never asked for.
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

    // Every arithmetic arm ADMITS both operands, computes WIDE, and re-admits the width: a result outside the
    // 32-bit range is a `MathError` carrying the saturated value, so the evaluator stays total where checked
    // arithmetic would raise.
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
    // pattern operand constructs an engine on EVERY event, which is exactly the per-event allocation the parser
    // being a value exists to foreclose. Backtracking is single-anchor, so a pattern of runs costs no exponential
    // retry and no caller injects an engine this grammar never defined.
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

// The grammar is built ONCE as static values: a parser is immutable, so constructing one per subscription
// rebuilds the whole expression graph for a text this fabric parses at admission and never again.
public static class Cesql {
    // `Rec` is the ONE self-reference seat: a static field cannot read a field whose initializer has not run, so
    // the term parser defers to `Atom` and `Atom` defers back through the parenthesised arm — the recursion the
    // grammar genuinely has, declared once.
    static readonly Parser<char, CesqlExpression> Expression =
        ExpressionParser.Build(Rec(static () => Atom), [
            [Operator.Prefix(Unary(CesqlOperator.Negate, "-")), Operator.Prefix(Unary(CesqlOperator.Not, "NOT"))],
            [Operator.InfixL(Infix(CesqlOperator.Multiply, "*")), Operator.InfixL(Infix(CesqlOperator.Divide, "/")), Operator.InfixL(Infix(CesqlOperator.Modulo, "%"))],
            [Operator.InfixL(Infix(CesqlOperator.Add, "+")), Operator.InfixL(Infix(CesqlOperator.Subtract, "-"))],
            // The two-glyph relational spellings precede their one-glyph prefixes at the SAME level, since `Or`
            // commits to a branch that consumed input and `<` would otherwise swallow the head of `<=`.
            [Operator.InfixN(Infix(CesqlOperator.LessEqual, "<=")), Operator.InfixN(Infix(CesqlOperator.GreaterEqual, ">=")),
             Operator.InfixN(Infix(CesqlOperator.Less, "<")), Operator.InfixN(Infix(CesqlOperator.Greater, ">"))],
            [Operator.InfixN(Infix(CesqlOperator.NotEqual, "<>")), Operator.InfixN(Infix(CesqlOperator.Equal, "=")), Operator.InfixN(Infix(CesqlOperator.Like, "LIKE"))],
            [Operator.InfixL(Infix(CesqlOperator.And, "AND"))],
            [Operator.InfixL(Infix(CesqlOperator.Xor, "XOR"))],
            [Operator.InfixL(Infix(CesqlOperator.Or, "OR"))],
        ]);

    // Every terminal is `Labelled`, so a refusal names this grammar's own vocabulary rather than a character
    // class; `Try` guards each shared-prefix alternation, since `Or` does not backtrack a branch that already
    // consumed input — a function name and a bare attribute share every leading glyph.
    static readonly Parser<char, CesqlExpression> Atom =
        OneOf(
            Try(Call).Labelled("<function-call>"),
            Try(Rec(static () => Expression).Between(Char('(').Between(SkipWhitespaces), Char(')').Between(SkipWhitespaces))),
            Literal.Labelled("<literal>"),
            Member.Labelled("<attribute>"))
        .Between(SkipWhitespaces);

    // Literals close on the three value spaces: a single-quoted string (doubled quote escapes), a signed
    // integer, and the two boolean words. `Num` is the specification's own integer shape, so no second numeric
    // grammar exists to disagree with the 32-bit width `CesqlValue` declares.
    static readonly Parser<char, CesqlExpression> Literal =
        OneOf(
            Try(AnyCharExcept('\'').ManyString().Between(Char('\''))).Map(static text => (CesqlExpression)new CesqlExpression.Literal(text)),
            Try(Num).Map(static value => (CesqlExpression)new CesqlExpression.Literal(value)),
            Try(CIString("TRUE")).ThenReturn((CesqlExpression)new CesqlExpression.Literal(true)),
            Try(CIString("FALSE")).ThenReturn((CesqlExpression)new CesqlExpression.Literal(false)));

    // An attribute name follows the branch grammar's own alphabet, and the optional `IN`/`NOT IN` tail is the
    // membership arm — one parser, because the set test is a property of the SUBJECT expression rather than a
    // second production a caller could reach without one.
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

    // A call resolves its NAME through the function table at PARSE time, so an unknown function refuses the
    // subscription rather than answering `MissingFunctionError` on every event of a subscription that can never
    // match — the one diagnostic the table lets the grammar hoist to admission.
    static readonly Parser<char, CesqlExpression> Call =
        from name in Letter.AtLeastOnceString().Labelled("<function-name>")
        from args in Rec(static () => Expression).Separated(Char(',').Between(SkipWhitespaces))
            .Between(Char('(').Between(SkipWhitespaces), Char(')').Between(SkipWhitespaces))
        from resolved in Resolved(name)
        select (CesqlExpression)new CesqlExpression.Call(resolved, toSeq(args));

    // The table's own lookup narrows to a proved row before the parser branches, so the resolution reads its
    // presence rather than asserting a nullable the compiler never narrowed.
    static Parser<char, CesqlFunction> Resolved(string name) =>
        CesqlFunction.TryGet(name, out CesqlFunction? row) && row is CesqlFunction found
            ? Return(found)
            : Parser<char>.Fail<CesqlFunction>($"<unknown-function:{name}>");

    // ONE admission: an unparseable expression refuses the SUBSCRIPTION here, so no delivery ever evaluates a
    // grammar this fabric could not read, and the rendered `ParseError` names the offset and the expectation set
    // the `Labelled` terminals published rather than a bare failure. `Before(End)` is what makes a trailing
    // fragment a refusal instead of a silently truncated predicate that matches more than its author wrote.
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

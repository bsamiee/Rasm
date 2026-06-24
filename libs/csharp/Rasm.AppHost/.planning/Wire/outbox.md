# [APPHOST_TRANSACTIONAL_OUTBOX]

The transactional-outbox and dead-letter owner for the runtime spine: a `DomainEvent` persists into a durable `Outbox` row in the SAME transaction as the producing write, a dispatch sweep on the one `SchedulePort` relays each pending row over an `OutboundHop` advancing a `(ConsumerId, Hlc)` watermark, a poison row exhausting its attempt budget routes to a `DeadLetter` lane, and the relay feeds the in-process `Wire/topics#BUS_CONDUCTOR` `EventBus.Dispatch` — so a decoupled domain event gains at-least-once dispatch with idempotent-key dedupe and exactly-once-effective delivery. The outbox row writes atomically with the producing transaction at `Rasm.Persistence` (the `ONE_OUTBOX_EGRESS_SPINE` ripple) and the workflow step-state row commits under the same tenant-scoped transaction (`SEAM_OUTBOX_AND_WORKFLOW_PERSISTENCE_TABLE`); the AppHost names the seam and the relay, atomicity stays Persistence. The page owns the outbox vocabulary, the dispatch sweep, the dead-letter lane, and the watermark-advancing relay; it consumes `DomainEvent`/`Topic`/`EventBus`, `DeliveryFanout`/`DeliveryReceipt` (the dedup-key precedent), `OutboundHop`/`OutboundSurface.Run` (the relay), `SchedulePort`/`ScheduleEntry.Spread` (the one sweep cadence), `HLC`/`EventLog` (ordering and the op-log), `FencingToken`, `TenantContext`, `ClockPolicy`, and `ReceiptSinkPort` as settled vocabulary, carries the durable outbox as a coordinated Persistence ripple, and mints no eighth port.

## [01]-[INDEX]

- [01]-[OUTBOX_FABRIC]: The transactional `OutboxRow`, the dispatch status, and the dead-letter lane.
- [02]-[DISPATCH_SWEEP]: The one `SchedulePort` sweep relaying pending rows over the watermark.
- [03]-[TS_PROJECTION]: Outbox-row and dead-letter wire shapes the dashboard consumes.

## [02]-[OUTBOX_FABRIC]

- Owner: `DispatchStatus` `[SmartEnum<string>]` the outbox-row lifecycle under the `LaneKeyPolicy` accessor; `OutboxRow` the durable transactional-outbox record; `DeadLetterRow` the poison-row record; `OutboxFault` `[Union]` fault family in the 4740 band.
- Cases: dispatch statuses pending | dispatched | dead-lettered; `OutboxFault` = Text | RelayRejected | Exhausted | WatermarkStale.
- Entry: `OutboxRow.Enqueue(DomainEvent evt, TenantContext tenant, Instant at)` materializes a pending row carrying the event payload, the topic, the dedup key, the HLC stamp, and a zero attempt count; `OutboxRow.Relayed(OutboxRow row, Instant at)` folds a successful relay onto the row as `dispatched`, and `OutboxRow.Deferred(OutboxRow row, OutboxFault cause, Instant at)` increments the attempt and routes to `dead-lettered` when the attempt budget is exhausted.
- Auto: the outbox row writes same-transaction with the producing write so a domain event and its source state commit atomically — a crash between the state write and the event publish cannot lose the event because both ride one transaction, and the dispatch sweep relays the durable row after commit, the transactional-outbox guarantee; the dedup key is the event's idempotency key so a re-enqueued identical event within the relay window dedupes through the `DeliveryFanout` cell exactly as the bus and notification fan-out dedupe, never a second dedup map; a row exhausting its attempt budget routes to `DeadLetterRow` carrying the last fault and the attempt history so a poison message leaves the dispatch lane rather than blocking it, and a dead-letter row is replayable through an operator command; the row carries the HLC stamp so the relay advances a `(ConsumerId, Hlc)` watermark monotonically and a relayed row never re-relays.
- Receipt: a relayed row mints one `DeliveryReceipt` (the `DeliveryFanout` shape) carrying the topic and the dispatched flag; a dead-letter transition fans one `SpineLog` event; no parallel outbox receipt.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, BCL inbox
- Growth: one dispatch status is one `DispatchStatus` row; a new outbox column is one field on `OutboxRow`; a new fault is one `OutboxFault` case; zero new surface.
- Boundary: the outbox is the only transactional-message owner — a fire-and-forget publish, a separate message queue, and a parallel event store are the deleted forms; the outbox row writes atomically with the producing transaction so atomicity stays Persistence and the AppHost names the seam — the durable outbox table, the dispatch-sweep cursor, and the dedup-key index land as the branch `ONE_OUTBOX_EGRESS_SPINE` Persistence ripple under the `TenantId` RLS predicate; the outbox row and the `Runtime/orchestration#STEP_STATE_SEAM` workflow step-state row commit under one tenant-scoped transaction so exactly-once-effective delivery and crash-durable step resumption share one durable boundary (`SEAM_OUTBOX_AND_WORKFLOW_PERSISTENCE_TABLE`); the relay registers as one keyed `OutboundHop` consumer advancing its own `(ConsumerId, Hlc)` watermark over the `ONE_OUTBOX_EGRESS_SPINE` op-log; the `[ONE_OUTBOX_EGRESS_SPINE]` branch binds three keyed `OutboundHop` consumers over the one op-log — this outbox relay, the `Runtime/orchestration#STEP_STATE_SEAM` workflow-step dispatch, and the `Rasm.Persistence/Sync/egress` webhook/gRPC sinks (registered through the `Runtime ⇄ Rasm.Persistence/Sync/egress # [PORT]: keyed OutboundHop egress` seam) — each draining the same `CdcEnvelopeWire` CloudEvents envelope as the hop payload rather than re-minting a second envelope shape or a parallel egress table, so the CloudEvents projection is the one wire payload and a per-consumer changefeed is the drift defect; the dedup reuses the `DeliveryFanout` idempotency-key precedent so the outbox dedup and the delivery dedup are one cell, never two.

```csharp signature
[SmartEnum<string>]
[KeyMemberEqualityComparer<LaneKeyPolicy, string>]
[KeyMemberComparer<LaneKeyPolicy, string>]
public sealed partial class DispatchStatus {
    public static readonly DispatchStatus Pending = new("pending");
    public static readonly DispatchStatus Dispatched = new("dispatched");
    public static readonly DispatchStatus DeadLettered = new("dead-lettered");
}

[Union]
public abstract partial record OutboxFault : Expected, IValidationError<OutboxFault> {
    private OutboxFault(string detail, int code) : base(detail, code, None) { }
    public static OutboxFault Create(string message) => new Text(message);
    public sealed record Text : OutboxFault { public Text(string detail) : base(detail, 4740) { } }
    public sealed record RelayRejected : OutboxFault { public RelayRejected(string detail) : base(detail, 4741) { } }
    public sealed record Exhausted : OutboxFault { public Exhausted(string detail) : base(detail, 4742) { } }
    public sealed record WatermarkStale : OutboxFault { public WatermarkStale(string detail) : base(detail, 4743) { } }
}

public sealed record OutboxRow(
    string OutboxId,
    string Topic,
    string DedupKey,
    JsonElement Payload,
    DispatchStatus Status,
    int Attempt,
    ulong Logical,
    Instant Physical,
    TenantContext Tenant) {
    public const int MaxAttempts = 8;

    public static OutboxRow Enqueue(DomainEvent evt, TenantContext tenant, Instant at) =>
        new($"{evt.Topic}:{evt.IdempotencyKey}", evt.Topic, evt.IdempotencyKey, evt.Payload, DispatchStatus.Pending, Attempt: 0, evt.Logical, evt.Physical, tenant);

    public OutboxRow Relayed(Instant at) => this with { Status = DispatchStatus.Dispatched };

    public OutboxRow Deferred(Instant at) =>
        Attempt + 1 >= MaxAttempts ? this with { Status = DispatchStatus.DeadLettered, Attempt = Attempt + 1 } : this with { Attempt = Attempt + 1 };

    public DomainEvent ToEvent() => new(Topic, DedupKey, Payload, DataClassification.Operational, Logical, Physical);
}

public sealed record DeadLetterRow(
    string OutboxId,
    string Topic,
    JsonElement Payload,
    string LastFault,
    int Attempts,
    Instant At);
```

## [03]-[DISPATCH_SWEEP]

- Owner: `OutboxRelay` the static sweep-and-relay surface over the one `SchedulePort` cadence, advancing the `(ConsumerId, Hlc)` watermark.
- Entry: `Sweep(OutboxRelay.Runtime runtime, TenantContext tenant, ulong watermark)` returns `IO<Seq<DeliveryReceipt>>` — reads the pending outbox rows past the `watermark` cursor, relays each through the in-process `EventBus.Dispatch` and the durable `OutboundHop`, advances the watermark on each success, and defers or dead-letters a failed row; `Replay(OutboxRelay.Runtime runtime, string outboxId)` returns `IO<DeliveryReceipt>` — re-enqueues a dead-letter row for one more dispatch attempt.
- Auto: the sweep rides one `ScheduleEntry.Spread` row on the one `SchedulePort` so the dispatch cadence is one schedule row, never a second scheduler — the fleet-spread seed distributes the sweep across nodes so two nodes do not relay the same row simultaneously, and the `FencingToken` fences the watermark advance so a stale node cannot rewind it; each pending row relays through `EventBus.Dispatch` to feed the in-process bus and through `OutboundSurface.Run` over its topic's `OutboundHop` for a durable subscriber, so the in-process and durable delivery legs ride one relay; a successful relay advances the `(ConsumerId, Hlc)` watermark monotonically so a relayed row never re-relays — the at-least-once-with-watermark guarantee that, with the consumer-side dedup, is exactly-once-effective; a failed relay increments the row's attempt and re-defers to the next sweep, routing to dead-letter on budget exhaustion so a poison row leaves the lane.
- Receipt: each relayed row mints one `DeliveryReceipt` carrying the topic and the dispatched flag; the watermark advance is the relay's own evidence; a dead-letter transition fans one `SpineLog` event; no parallel relay receipt.
- Packages: LanguageExt.Core, NodaTime, System.IO.Hashing, BCL inbox
- Growth: a new relay target is one `OutboundHop` the topic binds; the sweep cadence is one `ScheduleEntry.Spread` row column; zero new surface.
- Boundary: the dispatch sweep is the only outbox-relay owner — a per-row background loop, a second scheduler for the sweep, and a parallel relay are the deleted forms; the sweep rides the one `SchedulePort` so the cadence is one schedule row and the fleet-spread seed distributes it; the relay registers as one keyed `OutboundHop` consumer advancing its own `(ConsumerId, Hlc)` watermark over the `ONE_OUTBOX_EGRESS_SPINE` op-log, never re-minting the `CdcEnvelopeWire` or a second egress table; the watermark advance fences through `FencingToken.Admits` so two nodes cannot both advance it past one row; the consumer-side dedup reuses the `DeliveryFanout` cell so at-least-once dispatch plus idempotent-key dedup is exactly-once-effective, never an exactly-once distributed-transaction protocol.

```csharp signature
public static class OutboxRelay {
    public sealed record Runtime(
        EventBus.Cell Bus,
        OutboundRuntime Outbound,
        Func<TenantContext, ulong, Fin<Seq<OutboxRow>>> Pending,
        Func<OutboxRow, FencingToken, Fin<ulong>> Advance,
        Func<DeadLetterRow, Fin<Unit>> DeadLetter,
        Func<TenantContext, Fin<FencingToken>> Fence,
        Func<string, OutboundHop> Hop,
        Func<OutboxRow, DomainEvent, Func<CancellationToken, Task<HopOutcome>>> Send,
        ClockPolicy Clocks,
        ReceiptSinkPort Sink);

    public static IO<Seq<DeliveryReceipt>> Sweep(Runtime runtime, TenantContext tenant, ulong watermark) =>
        runtime.Pending(tenant, watermark).Match(
            Succ: rows => rows.TraverseM(row => Relay(runtime, tenant, row)).As(),
            Fail: fault => IO.pure(Seq<DeliveryReceipt>()));

    static IO<DeliveryReceipt> Relay(Runtime runtime, TenantContext tenant, OutboxRow row) =>
        from _bus in EventBus.Dispatch(runtime.Bus, row.ToEvent())
        from receipt in OutboundSurface.Run(runtime.Outbound, runtime.Hop(row.Topic), runtime.Send(row, row.ToEvent()))
        from _advance in receipt.Outcome is HopOutcome.Delivered
            ? IO.lift(() => runtime.Fence(tenant).Bind(token => runtime.Advance(row.Relayed(runtime.Clocks.Now), token)))
            : Defer(runtime, row)
        select new DeliveryReceipt(row.Topic, row.DedupKey, receipt.Outcome, Deduped: false, receipt.Attempts, receipt.Elapsed, Correlation.Mint());

    static IO<Fin<ulong>> Defer(Runtime runtime, OutboxRow row) =>
        row.Deferred(runtime.Clocks.Now) is var deferred && deferred.Status == DispatchStatus.DeadLettered
            ? IO.lift(() => runtime.DeadLetter(new DeadLetterRow(row.OutboxId, row.Topic, row.Payload, "relay-exhausted", deferred.Attempt, runtime.Clocks.Now)).Map(static _ => 0UL))
            : IO.pure(Fin.Succ(0UL));
}
```

```mermaid
sequenceDiagram
    participant Tx as Producing transaction
    participant Outbox as OutboxRow (same-tx)
    participant Sweep as OutboxRelay.Sweep
    participant Bus as EventBus.Dispatch
    participant Hop as OutboundHop
    Tx->>Outbox: Enqueue(event) [one transaction]
    Note over Outbox: committed atomically with source state
    Sweep->>Outbox: read pending past watermark
    Sweep->>Bus: Dispatch(event) [in-process leg]
    Sweep->>Hop: Run(event) [durable leg]
    Hop-->>Sweep: Delivered
    Sweep->>Outbox: advance (ConsumerId, Hlc) watermark [fenced]
```

## [04]-[TS_PROJECTION]

- Owner: `OutboxRowWire`, `DeadLetterRowWire` — the outbox-row and dead-letter wire shapes the dashboard ingests; the per-relay `DeliveryReceipt`s ride the existing `Wire/outbound#DELIVERY_FANOUT` `DeliveryReceiptWire`, bound here, never re-authored.
- Packages: BCL inbox
- Growth: one wire-member row per new outbox or dead-letter field; the dispatch status crosses as its smart-enum key; zero new surface.
- Boundary: the dispatch status crosses as its smart-enum string key; the HLC stamp crosses through the existing `HlcStampWire` so the outbox ordering reads the same causal primitive the receipt envelope carries; instants cross as extended-ISO text; the dead-letter row carries the last fault and attempt count so the dashboard surfaces a poison message for operator replay.

```ts contract
type DispatchStatusKey = "pending" | "dispatched" | "dead-lettered";

interface OutboxRowWire {
  readonly outboxId: string;
  readonly topic: string;
  readonly dedupKey: string;
  readonly status: DispatchStatusKey;
  readonly attempt: number;
  readonly logical: number;
  readonly physical: string;
}

interface DeadLetterRowWire {
  readonly outboxId: string;
  readonly topic: string;
  readonly lastFault: string;
  readonly attempts: number;
  readonly at: string;
}
```

## [05]-[RESEARCH]

- [OUTBOX_RIPPLE]: the durable transactional-outbox table, the dispatch-sweep cursor, and the dedup-key index are the `Rasm.Persistence` `ONE_OUTBOX_EGRESS_SPINE` ripple under the `TenantId` RLS predicate — the AppHost names the seam and the relay, the outbox row writes atomically with the producing transaction at Persistence, and the `[ONE_OUTBOX_EGRESS_SPINE]` branch binds three keyed `OutboundHop` consumers over the one op-log: this outbox relay, the `Runtime/orchestration#STEP_STATE_SEAM` workflow-step dispatch, and the `Rasm.Persistence/Sync/egress#EGRESS_SINK` webhook/gRPC sinks through the `[PORT]: keyed OutboundHop egress` seam — each advancing its own `(ConsumerId, Hlc)` watermark and draining the same `CdcEnvelopeWire` CloudEvents envelope as the hop payload rather than re-minting a second envelope shape or a parallel egress table, so the CloudEvents projection is the one wire payload across all three consumers; the outbox row and the `Runtime/orchestration#STEP_STATE_SEAM` workflow step-state row commit under one tenant-scoped transaction (`SEAM_OUTBOX_AND_WORKFLOW_PERSISTENCE_TABLE`), so exactly-once-effective delivery and crash-durable step resumption share one durable boundary that lands in parallel as the two Persistence legs.
- [SWEEP_CADENCE]: the dispatch sweep rides one `Runtime/time#SCHEDULE_PORT` `ScheduleEntry.Spread` row so the cadence is one schedule row and the fleet-spread `XxHash3` seed distributes the sweep across nodes — a second scheduler for the outbox sweep is the rejected form; the watermark advance fences through `Runtime/time#FENCING_TOKEN` `FencingToken.Admits` so two nodes cannot both advance it past one row, and the consumer-side dedup reuses the `Wire/outbound#DELIVERY_FANOUT` idempotency-key precedent so at-least-once dispatch plus idempotent-key dedup is exactly-once-effective, never an exactly-once distributed-transaction protocol.
- [BUS_FEED]: the relay feeds the in-process `Wire/topics#BUS_CONDUCTOR` `EventBus.Dispatch` and the durable `OutboundHop` over `OutboundSurface.Run` in one relay, so the in-process leg carries bounded back-pressure and the durable leg rides the one retry owner; the `DeadLetter` lane carries a poison row for operator replay through `Replay`, never a blocked dispatch lane.

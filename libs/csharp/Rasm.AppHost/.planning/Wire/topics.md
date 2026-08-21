# [APPHOST_EVENT_BUS_TOPICS]

Rasm.AppHost runs one in-process event bus on the runtime spine: a `Topic` row stamps each `DomainEvent` with its topic's next dense `Offset` and fans it to bounded consumer sinks, each subscription seated directly as a fan sink so one hop owns exactly one waiting room. This page owns the topic and subscription topology, the per-topic delivery ordinal, the gap-and-residual fold accounting every declined offer, the per-topic durability and shed columns, and the in-process delivery the outbox dispatch sweep feeds.

Composition takes `DrainSurface`/`DrainKind`/`DrainSpec`/`DrainQueue`/`DrainBand`/`DrainRow`/`FanMiss` from `Runtime/resources#DRAIN_QUEUES` and `Runtime/lifecycle#DRAIN_CONDUCTOR`, the `Runtime/resources#DEDUPE_WINDOW` `DedupeWindow`, `DeliveryReceipt`/`DeliveryDisposition`, `OutboundSurface.Run`/`OutboundHop`, `HLC`/`EventLog`, `DegradationLevel`, `CancelScope`, `ClockPolicy`, `ReceiptKind`, and `ReceiptSinkPort` as settled vocabulary.

`System.Threading.Tasks.Dataflow` resolves from the `net10.0` shared framework, so no manifest row and no package reference carry it, and this page mints no eighth port.

## [01]-[INDEX]

- [02]-[TOPIC_FABRIC]: `Topic` rows over a `DrainSurface.Broadcast` fan, the `DomainEvent` carrier, and the drop-receipt vocabulary.
- [03]-[SUBSCRIPTION_FABRIC]: Bounded consumer sinks, the gap fold over dense offsets, and the composed dedupe window.
- [04]-[BUS_CONDUCTOR]: One conductor folding topics and subscriptions under the drain band, shedding by rank and closing tail loss at drain.
- [05]-[TS_PROJECTION]: Drop-receipt wire face the loss dashboard consumes.

## [02]-[TOPIC_FABRIC]

- Owner: `DomainEvent` the topic-carrying event carrier stamped with its topic's delivery ordinal, carrying the `EventType` fact identity and `EventSource` producer identity the durable projection publishes; `Topic` `[SmartEnum<string>]` the topic axis under the `ComparerAccessors.StringOrdinal` accessor carrying its `TopicDurability` and its shed threshold; `TopicDurability` `[SmartEnum<string>]` the two-row durability axis the outbox enqueue reads; `DropClass` `[SmartEnum<string>]` the two-row loss axis; `DropReceipt` the one loss evidence both classes mint; `TopicHead` the fan capsule carrying the topic's `Atom<ulong>` offer cursor; `BusFault` `[Union]` fault family in the 4730 band.
- Cases: topic rows are the declared event channels — `Command`, `Lifecycle`, `Health`, `Delivery`, and the two `Rasm.AppUi` `Collab/sync` channels, `Collab` carrying the durable document deltas and `Presence` the lossy presence and awareness frames — each binding its `TopicDurability` arm and the `Option<DegradationLevel>` rank at which its delivery sheds; `TopicDurability` = durable | ephemeral; `DropClass` = shed | missed; `BusFault` = TopicUnknown.
- Law: `Topic` and `EventType` are TWO vocabularies over ONE fact and the relation between them is a PROJECTION, never a derivation — the topic names which in-process fan carries a delivery and what it sheds at, the type names the fact a peer subscribes on, one channel serves many types, and one type routes differently per deployment. `Wire/outbox#OUTBOX_FABRIC` holds the single projection from this carrier onto the CloudEvents envelope, so a second mapping table anywhere is the drift defect and no consumer re-derives one from the other.
- Law: the carrier holds the topic ROW, never its key — the durability arm, the shed rank, and the fan the row names are all facts the consumer needs, and a key beside them makes every reader re-resolve the roster it was already handed. Keys cross at the wire alone, where a peer holds no roster.
- Law: an event name refuses at THIS mint — `SignalGovernance.Rostered` resolves `EventType.Domain` against the branch capability roster, so a subject reaching a broker unjoined to its own metrics never constructs, and the gate sits at the declaration owner rather than at the egress edge an ephemeral topic slips past.
- Law: the HLC `(Physical, Logical)` pair and `Offset` are TWO facts rather than two spellings of one — HLC orders causally ACROSS topics and carries no density, while `Offset` counts deliveries WITHIN one topic and is dense by construction because `Publish` is its only stamper; a gap read off HLC `Logical` cannot separate a lost delivery from a concurrent producer's stamp, so delivery accounting reads `Offset` alone and causal replay reads the pair alone.
- Law: loss classes stay DISJOINT — `Shed` names what the degradation gate refused before the fan and rises with producer load, `Missed` names what a bounded sink declined at the fan and rises with consumer lag; one merged tally erases the only signal separating producer-hot from consumer-slow, so no receipt spans both and no consumer sums them.
- Entry: `Open(Topic topic, Seq<ITargetBlock<DomainEvent>> sinks, Action<FanMiss> missed, CancellationToken token)` returns `Fin<TopicHead>` — mints the topic's fan through `DrainSurface.Broadcast` over the subscription consumers already built, so the builder owns both the block and every link, and the drain owner refuses a fan-out row carrying no loss reporter; `Publish(TopicHead head, DomainEvent evt)` returns `IO<Unit>` — advances the head's cursor, stamps the ordinal onto the copy it hands the fan, and posts under the fan row's bounded back-pressure so a fast producer awaits fullness rather than dropping.
- Auto: the topic fan is one `DrainSurface.Broadcast` builder over the `DrainSpec.ReceiptFanOut` row, so the block, its `LinkOptions` `PropagateCompletion`, and its `Tail.Completion` are the drain owner's, never a hand-rolled fan-out loop beside them; the clone delegate detaches the payload — `JsonElement.Clone()` copies the element out of its parent `JsonDocument` so a subscription holding a received event past the producer's document lifetime reads live bytes rather than a recycled buffer; `Publish` advances the head's `Atom<ulong>` cursor and stamps the post-swap value onto the copy it hands out, so the sequence reaching every sink is dense and monotone per topic and the first published event carries one; the event also carries its HLC stamp so subscriptions replay causally by the `(Physical, Logical)` pair the `Runtime/determinism#EVENT_LOG` chain and the `ReceiptEnvelope` carry; `Publish` rides the fan row's `BoundedCapacity` so a fast producer awaits on a `Wait` row rather than dropping — back-pressure is the bound at the HEAD alone, never unbounded accumulation and never a promise about what a bounded sink does with the head's offer; the fan row's `DrainBand` seats every topic under the conductor so a topic completes at its declared drain band on unload.
- Receipt: a published event is one `DomainEvent` on the fan; a declined offer accounts as one `Missed` `DropReceipt` naming the inclusive ordinal span, minted by the subscription's gap fold in flight and by the conductor's residual at drain through the topic's one `FanMiss` projection; per-subscription delivery rides the subscription's own `DeliveryReceipt`; no parallel topic receipt.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Rasm (the `Rasm/Domain/event` grammar owner supplying `EventType`/`EventSource`), BCL inbox
- Growth: one topic is one `Topic` row binding its durability arm and shed threshold; a new fact is one `EventType` a producer composes through the kernel grammar and none here; a new event shape is one `DomainEvent` payload column; a third loss class earns a `DropClass` row only by naming a cause the two rows merge, never by naming a new site; zero new surface.
- Boundary: the topic fabric is the only in-process pub/sub owner — a per-topic background loop, a hand-rolled fan-out, and a second queue owner are the deleted forms; the fan is a `DrainSurface.Broadcast` builder over the one `DrainKind` union so the Dataflow `BroadcastBlock` reaches this page only through `Runtime/resources#DRAIN_QUEUES`, one Dataflow owner for the whole spine, and the producer reaches the intake through a total arm projection at the publish site rather than a raw block retained beside the queue; the fan spec and the sink spec are PAGE constants rather than a per-topic column, because all six rows named one value each and a column holding one value across its whole roster is a knob wearing a row — a topic that genuinely earns its own back-pressure row takes the column back with the discriminant named; causal ordering is the HLC stamp the suite already carries so the bus and the command log order by one primitive, never a re-minted timeline, while `Offset` is delivery accounting alone and never crosses into ordering — gap detection over HLC `Logical` is the deleted form, because HLC logical is not dense per topic and a normal jump is then indistinguishable from a lost delivery; producer back-pressure is the fan's own `BoundedCapacity` — `Publish`'s `SendAsync` awaits when the bounded head is full — while the `BroadcastBlock` is latest-value to a slow target by construction, so the in-process leg is bounded best-effort fan-out and the AT-LEAST-ONCE guarantee is the durable `Wire/outbox#DISPATCH_SWEEP` leg (the outbox row + watermark + consumer dedup), never the in-process broadcast; a subscription whose bounded consumer declines the fan's offer therefore misses the in-process copy, and that miss is EVIDENCE rather than silence — the gap fold and the drain residual account it at both ends, so no declined delivery leaves the bus unrecorded on any topic; the outbox re-receive is the `TopicDurability.Durable` row's guarantee alone — an `Ephemeral` row is NEVER enqueued, so the outbox sweep reads the durability column rather than every topic, the receipt's `Resent` verdict derives from that one column, and a claim of per-topic loss classes expressed in no column is the deleted form; the `Rasm.AppUi` `Collab/sync` live-delta broadcast rides these topic rows as OPAQUE `DomainEvent.Payload` bytes — the session-ephemeral CRDT wire is the subscriber's to decode and never durable truth here — with the document deltas on the `Durable` `Collab` row riding the outbox leg and the `Ephemeral` `Presence` frames staying in-process, so an awareness frame a slow subscriber misses is receipted and final by design and a second bus for collaboration is the deleted form.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class TopicDurability {
    public static readonly TopicDurability Durable = new("durable");
    public static readonly TopicDurability Ephemeral = new("ephemeral");
}

// Two rows, two causes, never one tally. Shed counts dispatches the degradation gate refused ahead of the
// fan and tracks producer load; Missed counts offers a bounded sink declined at the fan and tracks consumer
// lag. Summing them yields a number no operator can act on.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class DropClass {
    public static readonly DropClass Shed = new("shed");
    public static readonly DropClass Missed = new("missed");
}

// Sheds is the degradation RANK at or above which this topic's delivery is dropped, and None is the arm a
// topic that never sheds takes — an absent threshold, never a sentinel rank a comparison would still admit.
// Presence is awareness traffic whose loss is already the design, so it sheds at the first reduced level.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class Topic {
    public static readonly Topic Command = new("command", TopicDurability.Durable, None);
    public static readonly Topic Lifecycle = new("lifecycle", TopicDurability.Durable, None);
    public static readonly Topic Health = new("health", TopicDurability.Ephemeral, None);
    public static readonly Topic Delivery = new("delivery", TopicDurability.Durable, None);
    public static readonly Topic Collab = new("collab", TopicDurability.Durable, None);
    public static readonly Topic Presence = new("presence", TopicDurability.Ephemeral, Some(DegradationLevel.ReducedRemote));

    public TopicDurability Durability { get; }

    public Option<DegradationLevel> Sheds { get; }
}

// --- [MODELS] -------------------------------------------------------------------------------
public sealed record DomainEvent(
    Topic Topic,
    EventType Type,
    EventSource Source,
    string IdempotencyKey,
    JsonElement Payload,
    DataClassification Classification,
    ulong Logical,
    Instant Physical,
    ulong Offset) {
    // TWO vocabularies, ONE projection. `Topic` is the in-process ROUTING channel — which fan carries this
    // delivery, what it sheds at, whether it is durable — while `EventType` is the FACT identity a peer
    // subscribes on, and neither derives from the other because a channel serves many facts and one fact may
    // route differently per deployment.
    //
    // Minting rails on `Fin` because Tier-0 `[08]-[OBSERVABILITY_CONFORMANCE]` binds EVENT names to the same
    // capability roster instrument names answer to: an unrostered subject refuses HERE, at its declaration
    // owner, rather than reaching a broker as a type no board can join to the metrics it is about.
    public static Fin<DomainEvent> Of(
        Topic topic, EventType type, EventSource source, string idempotencyKey, JsonElement payload,
        DataClassification classification, ulong logical, Instant physical) =>
        SignalGovernance.Rostered(type).Map(admitted =>
            new DomainEvent(topic, admitted, source, idempotencyKey, payload, classification, logical, physical, Offset: 0));

    // Clone() detaches the element onto its own storage, discharging the fan's copy guard: JsonElement over a
    // pooled JsonDocument stays valid only while that document lives, so a subscription outliving the
    // producer's parse reads a recycled buffer. An identity delegate hands every sink the same borrowed
    // window and proves no isolation at all.
    public static DomainEvent Detach(DomainEvent evt) => evt with { Payload = evt.Payload.Clone() };

    // Of stays a pure MINT and Publish the one stamper, because an ordinal is a property of ARRIVAL at one
    // topic's fan rather than of the event a caller composed: two producers building the same event never
    // agree on a number, and one fan handing out its own always does.
    public static DomainEvent Stamped(DomainEvent evt, ulong offset) => evt with { Offset = offset };
}

// Span and class are the whole record; count and re-send verdict DERIVE. A stored `Resent` beside the topic
// row disagrees with that row eventually and makes one loss read two ways, and a stored count beside an
// inclusive span is the same defect one arithmetic step away.
public sealed record DropReceipt(
    Topic Topic,
    Option<string> Subscription,
    DropClass Class,
    ulong First,
    ulong Last) {
    public ulong Count => Last - First + 1;

    public bool Resent => Topic.Durability == TopicDurability.Durable;

    // Missed projects the drain owner's payload-agnostic FanMiss, so the span arrives already inclusive on
    // both ends. Shed carries no subscription at all, because the degradation gate refused the dispatch
    // before any fan offered it; its span pins the head cursor, which locates the refusal inside the topic's
    // own sequence rather than inventing an ordinal nothing stamped.
    public static DropReceipt Missed(Topic topic, FanMiss miss) =>
        new(topic, Some(miss.Sink), DropClass.Missed, miss.First, miss.Last);

    public static DropReceipt Shed(Topic topic, ulong cursor) =>
        new(topic, None, DropClass.Shed, cursor, cursor);
}

public sealed record TopicHead(Topic Topic, DrainQueue<DomainEvent> Fan, Atom<ulong> Cursor);

// --- [ERRORS] ---------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record BusFault : Fault {
    private static readonly FaultBand FamilyBand = FaultBand.Bus;
    private BusFault(string detail) => Detail = detail;
    public string Detail { get; }
    public sealed override string Message => Detail;

    [FaultCase(0)]
    public sealed partial record TopicUnknown : BusFault { public TopicUnknown(string detail) : base(detail) { } }
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class TopicFabric {
    // One fan row and one sink row for the whole roster: every topic declared the same back-pressure value,
    // so the column was a knob and these are the two policies the bus actually has.
    public static readonly DrainSpec Fan = DrainSpec.ReceiptFanOut;
    public static readonly DrainSpec Sink = DrainSpec.SubscriptionSink;

    // Callers build every subscription consumer BEFORE the fan, so the builder owns every link: the head
    // links to each sink under the row's PropagateCompletion and exposes itself as intake and Tail.
    public static Fin<TopicHead> Open(Topic topic, Seq<ITargetBlock<DomainEvent>> sinks, Action<FanMiss> missed, CancellationToken token) =>
        Fan.Broadcast(DomainEvent.Detach, sinks, Some(missed), token)
            .Map(fan => new TopicHead(topic, fan, Atom(0UL)));

    // Ordinal burns on the offering arm alone, so a topology mismatch never punches a hole in the sequence
    // that gap fold reads as loss; Swap returns the POST-swap value, so the first published event carries one
    // and zero stays the empty seat a subscription opens on rather than a delivered offer.
    public static IO<Unit> Publish(TopicHead head, DomainEvent evt) =>
        head.Fan.Switch(
            state: (Head: head, Event: evt),
            pipe: static (_, p) => IO.fail<Unit>(new DrainFault.TopologyMismatch(p.Spec.Name, DrainKind.FanOut.Key)),
            network: static (s, n) => IO.liftAsync(async () => {
                await n.Intake.SendAsync(DomainEvent.Stamped(s.Event, s.Head.Cursor.Swap(static held => held + 1))).ConfigureAwait(false);
                return unit;
            }));
}
```

Every row orders on the HLC `(Physical, Logical)` pair, counts deliveries on its own dense `Offset`, and bounds on the one fan row's `BoundedCapacity`, so those three coordinates carry no per-row signal and stay out of the table; guarantee and give-up clause are where these rows genuinely diverge.

| [INDEX] | [TOPIC]     | [DELIVER]                     | [DEGRADE]                                                   |
| :-----: | :---------- | :---------------------------- | :---------------------------------------------------------- |
|  [01]   | `Command`   | at-least-once via the outbox  | a declined offer receipts `Missed`; the sweep resends       |
|  [02]   | `Lifecycle` | at-least-once via the outbox  | a declined offer receipts `Missed`; the sweep resends       |
|  [03]   | `Health`    | best-effort, never enqueued   | a declined offer receipts `Missed`; nothing resends         |
|  [04]   | `Delivery`  | at-least-once via the outbox  | a declined offer receipts `Missed`; the sweep resends       |
|  [05]   | `Collab`    | at-least-once via the outbox  | opaque payload; a miss receipts `Missed`, the sweep resends |
|  [06]   | `Presence`  | best-effort, sheds on reduced | `Shed` at reduced, `Missed` at the fan; neither resends     |

## [03]-[SUBSCRIPTION_FABRIC]

- Owner: `Subscription` the subscriber capsule over ONE bounded consumer sink seated as a fan sink, carrying its `Atom<Received>` offset seat; `Received` the transition record that seat swaps through; `SubscriptionFabric` the static open-and-link surface over the `DrainSurface` builders.
- Law: each hop owns exactly one waiting room — the sink row's own `BoundedCapacity` IS the subscription's back-pressure, and a second bounded block in front of it splits one hop's pressure evidence across two policies while making the fan's decline no more observable than before.
- Law: the gap fold is CONSERVATION, never interception — an arriving `Offset` more than one past the seat proves every ordinal between them reached the fan and was refused, which is the only account available because a `BroadcastBlock` overwrites per target and reports no decline to its source.
- Law: the subscription NAMES itself. Keys derived from the topic and its back-pressure row collided the moment two subscribers on one topic shared a row, which every subscriber now does — one seat, two names, and a gap fold reporting one subscriber's loss under the other's identity.
- Entry: `Open(Topic topic, string name, Func<DomainEvent, IO<Unit>> consume, Action<FanMiss> missed, DedupeWindow dedupe, ClockPolicy clocks, CancellationToken token)` returns `Subscription` — opens the consumer sink through `DrainSpec.SubscriptionSink.ActionSink`, folds each arriving offset into the seat before the dedupe window decides, and hands the queue's own `Intake` to `TopicFabric.Open` as one of the fan's sinks, so the subscription exists before the topic it drains.
- Auto: each subscription is one `DrainQueue<DomainEvent>` opened through the drain owner's consumer-sink builder, so the block's bound, degree, ordering, and completion are all the sink row's declared policy and no package handle escapes onto this record; the topic fan links it under `PropagateCompletion` so a topic completion fans completion to every subscription; the sink row drains at degree one and ordered, so the seat advances monotonically and a backward arrival is a re-offer rather than a reordering; the seat rides one compare-and-swap so two fan deliveries racing one subscription never both claim one span; the dedupe is the `Runtime/resources#DEDUPE_WINDOW` `DedupeWindow` value the composition hands in — `Admit(key, clocks.Now)` prunes, decides, and records inside one compare-and-swap, so a re-published identical event inside the window folds to a no-op before the consumer runs and two fans racing one key admit exactly one; the instant is the threaded `ClockPolicy`'s, so a fake-clock spec expires the window deterministically.
- Receipt: each consumed event mints one `DeliveryReceipt` (the `Wire/outbound#DELIVERY_FANOUT` shape) carrying the subscription key and the dedupe verdict on its disposition; each forward jump in the arriving `Offset` mints one `FanMiss` the topic's projection turns into a `Missed` `DropReceipt`; a backward arrival counts on the seat's own re-offer tally and mints nothing, because the dedupe window is what absorbs it and a second account of one re-offer is the merged tally this page refuses everywhere else; no parallel subscription receipt.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, BCL inbox
- Growth: one subscription is one `Open` call over a topic row; a new back-pressure class is one `DrainSpec` row at the drain owner; zero new surface.
- Boundary: the subscription is a `DrainSurface` builder over the one `DrainKind` union — a per-subscription background loop, a second queue owner, and a raw `new ActionBlock` mint are the deleted forms, and the public block field this record once published was that last one wearing a capsule: a package handle whose capacity, degree, ordering, and completion no row stated; a bounded `BufferBlock` in front of the consumer is deleted twice over, since `docs/stacks/csharp/domain/concurrency.md` `[BLOCK_ADMISSION]` rejects that block outright and it buys nothing anyway — a bounded `BufferBlock` declines the fan's offer exactly as silently as the bounded sink does, so it adds a waiting room and no evidence; a hand-written receipting `ITargetBlock<DomainEvent>` intercepting each offer is likewise deleted, because it re-implements the `consumeToAccept`, `ConsumeMessage`, and postponement protocol the drain owner deliberately declines to own, against an admission rail that admits blocks on four named capabilities and never on a hand-written target; the bound is the loss boundary the at-least-once outbox leg closes for a `Durable` row and the receipted, final loss for an `Ephemeral` one, never unbounded accumulation; the dedupe composes the ONE `DedupeWindow` primitive the delivery fan-out also composes, so the bus dedupe and the notification dedupe are one owner and one window-bound — a local seen-key map beside it is the deleted form; replay ordering is the HLC pair and delivery accounting is the `Offset` seat, two facts one hop apart that never collapse into one column; a correlated join or dual-stream coalesce over two event streams reaches `DrainSurface.Join`/`DrainSurface.Coalesce` at the drain owner directly, so a renaming forward on this page is the deleted form; the durable relay to a persistent subscriber rides the `OutboundHop` so the bus rides the one retry owner, and the outbox dispatch sweep feeds the topics over the `ONE_OUTBOX_EGRESS_SPINE` op-log (`Wire/outbox#OUTBOX_FABRIC`) so the durable leg of delivery feeds the in-process bus, never a second egress table.

```csharp signature
// --- [MODELS] -------------------------------------------------------------------------------
public sealed record Subscription(
    string Key,
    DrainQueue<DomainEvent> Consumer,
    Atom<Received> Seat);

// Verdict rides the TRANSITION rather than a read-then-write pair, the shape DedupeWindow.Admit already
// takes: one compare-and-swap advances the seat, widens the tally, and carries the span its caller receipts.
public readonly record struct Received(ulong Last, ulong Missed, ulong Reoffered, Option<(ulong First, ulong Last)> Gap) {
    public static readonly Received Empty = new(Last: 0, Missed: 0, Reoffered: 0, Gap: None);

    // Three arms, three facts. A FORWARD jump proves every ordinal between reached the fan and was refused.
    // Contiguous advances move the seat alone. Repeats and backward arrivals under the ordered single-degree
    // sink are RE-OFFERS the fan made — the dedupe window absorbs them — and they count on their own column
    // rather than folding into `ulong.Max`, where a subscription re-offered one ordinal forever reads
    // byte-identically to one receiving nothing at all.
    public Received Seated(ulong arrived) =>
        arrived > Last + 1 ? new(arrived, Missed + arrived - Last - 1, Reoffered, Some((Last + 1, arrived - 1)))
        : arrived > Last ? new(arrived, Missed, Reoffered, None)
        : this with { Reoffered = Reoffered + 1 };
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class SubscriptionFabric {
    // ONE waiting room per hop: the drain owner's consumer sink IS the fan's sink, so its own row carries the
    // whole back-pressure and its own decline is the loss the seat accounts. Nothing here intercepts the
    // offer, because the broadcast head reports no decline to the source it fans from.
    // Seat and key mint ONCE, outside the consumer the sink runs per delivery — the same
    // mint-outside-the-transition law the kernel `Cell` family states, and the reason this one member carries
    // a statement body while every other on the page is an expression.
    public static Subscription Open(
        Topic topic, string name, Func<DomainEvent, IO<Unit>> consume, Action<FanMiss> missed,
        DedupeWindow dedupe, ClockPolicy clocks, CancellationToken token) {
        Atom<Received> seat = Atom(Received.Empty);
        string key = $"{topic.Key}:{name}";
        return new Subscription(
            key,
            TopicFabric.Sink.ActionSink<DomainEvent>(
                async evt => {
                    seat.Swap(held => held.Seated(evt.Offset)).Gap
                        .Iter(gap => missed(new FanMiss(TopicFabric.Fan.Name, key, gap.First, gap.Last)));
                    if (dedupe.Admit(evt.IdempotencyKey, clocks.Now)) {
                        await consume(evt).RunAsync().ConfigureAwait(false);
                    }
                },
                token),
            seat);
    }

    // Total arm projection: a subscription sink is a `Network` queue by construction, and the pipe arm is a
    // typed rail failure rather than a cast the fan would take on faith.
    public static Fin<Seq<ITargetBlock<DomainEvent>>> Intakes(Seq<Subscription> subs) =>
        subs.Traverse(static sub => sub.Consumer.Switch(
                pipe: static p => Fin.Fail<ITargetBlock<DomainEvent>>(
                    new DrainFault.TopologyMismatch(p.Spec.Name, DrainKind.Network.Key)),
                network: static n => Fin.Succ(n.Intake)))
            .As();
}
```

## [04]-[BUS_CONDUCTOR]

- Owner: `EventBus` the static conductor folding topics and their subscriptions into one bus, draining them under the `Runtime/lifecycle#DRAIN_CONDUCTOR` band and closing tail loss at that drain.
- Law: mounting ACCUMULATES — the topics are independent admissions and a `Fin` fold reported the first refusal alone, so a composition wiring six topics learned about one at a time; the fold is a `Validation` traverse and one pass names every topic that fails to mount.
- Entry: `Mount(EventBus.Runtime runtime, params ReadOnlySpan<(Topic Topic, Seq<(string Name, Func<DomainEvent, IO<Unit>> Consume)> Subscribers)> rows)` returns `Fin<EventBus.Cell>` — opens every subscriber, then the topic fan over their intakes, and registers one typed `DrainRow` per topic at the fan row's declared band; `Dispatch(EventBus.Cell cell, DomainEvent evt)` returns `IO<Unit>` — publishes an event to its topic fan under the live degradation gate, the one entry the outbox dispatch sweep and the in-process producers both invoke.
- Auto: the conductor opens each topic's subscribers first and its fan second so the topology is one builder call at mount, never per-publish and never a second link pass; each topic row mints ONE `FanMiss` projection threaded to the gap fold, the tail residual, and the drain owner's admission, so the loss class and the durability verdict fix once per topic; the bus registers one `DrainRow` per topic at the fan row's `DrainBand` so on unload the conductor completes each topic fan, fans completion to every subscription through `PropagateCompletion`, and awaits the consumers' completion UNDER THE CONDUCTOR'S OWN TOKEN, so the drain-forced escalation reaches an in-flight consumer instead of waiting on a completion nothing can cancel; after those completions the conductor compares the head's final cursor against each subscription's seat and mints the residual `Missed` receipt, which is the only account of a subscription that missed the LAST deliveries and therefore never sees a later ordinal; `Dispatch` routes an event to its topic ROW so a producer and the outbox sweep both publish through one entry with no key re-resolution, and it reads the live `DegradationLevel` against the topic's own `Sheds` rank so a reduced host drops awareness traffic and keeps every unthresholded channel.
- Receipt: the bus drain folds into the `DrainReceipt` the conductor mints; per-event delivery rides the subscriptions' `DeliveryReceipt`s; a shed dispatch is a receipted no-op minting one `Shed` `DropReceipt` rather than a fault; the drain residual mints the closing `Missed` receipts onto the same sink seat the in-flight gaps feed, and every `DropReceipt` fans one `DropReceiptWire` envelope under `ReceiptKind.Drop` so the bus's whole loss account reaches a board rather than an in-process delegate alone.
- Packages: Rasm (kernel `ReceiptSinkPort`), Riok.Mapperly, LanguageExt.Core, NodaTime, BCL inbox
- Growth: one topic-and-subscribers row absorbs a new event channel; a new producer drives the one `Dispatch`; zero new surface.
- Boundary: the bus conductor is the only multi-topic bus owner — a per-topic conductor, a parallel drain, and a second bus owner are the deleted forms; the bus drains under the one `DrainConductor` band so the bus completion and the runtime drain are one fold, never a bus-specific shutdown, and the participant registration is one typed `DrainRow` rather than four positional arguments whose rank every call site spelled as a literal — every topic fan is independent inside its band, so the rank is one page constant naming that independence rather than a zero repeated per mount; the cell carries its runtime rather than mirroring two of its columns, because a mirrored level reader and a mirrored drop sink are two facts with two owners that a re-composition can leave disagreeing; loss accounting closes at exactly two ends — a gap in flight and a residual at drain — so an interception layer between them is the deleted form and every declined offer lands on one of the two; the two `DropClass` rows never merge into one tally, so a bus-wide drop counter is the deleted form; the bus dispatch is the one entry the outbox dispatch sweep feeds and the in-process producers invoke so the durable and in-process delivery legs meet at one bus, never two; shedding is a per-topic rank comparison against the live level, so a `Suspended` host sheds exactly the topics whose rows declare a threshold and a blanket dispatch gate is the deleted form; the relay to a durable subscriber rides the `OutboundHop` over `OutboundSurface.Run` so the bus rides the one retry owner and the `DeliveryFanout` folds in as one subscriber rather than a parallel sender.

```csharp signature
// --- [COMPOSITION] --------------------------------------------------------------------------
public static class EventBus {
    // Every topic fan is independent inside its band, so all bus participants share one rank and the
    // conductor may drain them in any order — a per-mount literal asserted an ordering nothing needs.
    public const int FanRank = 0;

    public sealed record Runtime(
        DeliveryRuntime Delivery,
        Func<DegradationLevel> Level,
        Action<DropReceipt> Drops,
        Func<DrainRow, Unit> Register,
        ClockPolicy Clocks,
        CancelScope Spine);

    // Cells carry their runtime rather than two copies of its columns: a mirrored level reader and a
    // mirrored drop sink are one fact with two owners the moment a composition rebuilds either.
    public sealed record Cell(
        Runtime Runtime,
        HashMap<Topic, TopicHead> Heads,
        Seq<Subscription> Subscriptions);

    // --- [OPERATIONS] -------------------------------------------------------------------------
    public static Fin<Cell> Mount(
        Runtime runtime,
        params ReadOnlySpan<(Topic Topic, Seq<(string Name, Func<DomainEvent, IO<Unit>> Consume)> Subscribers)> rows) =>
        toSeq(rows.ToArray())
            .Traverse(row => Mounted(runtime, row).Match(
                Succ: Validation<Error, (TopicHead Head, Seq<Subscription> Subs)>.Success,
                Fail: Validation<Error, (TopicHead Head, Seq<Subscription> Subs)>.Fail))
            .As()
            .ToFin()
            .Map(mounted => new Cell(
                runtime,
                mounted.Fold(HashMap<Topic, TopicHead>.Empty, static (held, row) => held.Add(row.Head.Topic, row.Head)),
                mounted.Fold(Seq<Subscription>(), static (held, row) => held + row.Subs)));

    static Fin<(TopicHead Head, Seq<Subscription> Subs)> Mounted(
        Runtime runtime, (Topic Topic, Seq<(string Name, Func<DomainEvent, IO<Unit>> Consume)> Subscribers) row) {
        Action<FanMiss> missed = Missed(runtime.Drops, row.Topic);
        Seq<Subscription> subs = row.Subscribers.Map(sub => SubscriptionFabric.Open(
            row.Topic, sub.Name, sub.Consume, missed, runtime.Delivery.Dedupe, runtime.Clocks, runtime.Spine.Token));
        return from intakes in SubscriptionFabric.Intakes(subs)
               from head in TopicFabric.Open(row.Topic, intakes, missed, runtime.Spine.Token)
               select (Seated(runtime, row.Topic, head, subs, missed), subs);
    }

    static TopicHead Seated(Runtime runtime, Topic topic, TopicHead head, Seq<Subscription> subs, Action<FanMiss> missed) =>
        (runtime.Register(new DrainRow(
            Name: $"bus:{topic.Key}",
            Band: TopicFabric.Fan.Band,
            Rank: FanRank,
            Drain: token => Drain(head, subs, missed, token))), head).Item2;

    // ONE projection per topic row, minted before either the subscriptions or the fan exist and threaded to
    // all three seats that report: gap fold, tail residual, and the drain owner's admission.
    static Action<FanMiss> Missed(Action<DropReceipt> drops, Topic topic) =>
        miss => drops(DropReceipt.Missed(topic, miss));

    // Shedding returns unit rather than a fault: the level ruled the drop, so the producer is not in error
    // and has nothing to retry. Receipt mints anyway on its own class, because producer-side shed and
    // consumer-side miss diverge in cause and one merged tally erases the only signal separating them.
    public static IO<Unit> Dispatch(Cell cell, DomainEvent evt) =>
        cell.Heads.Find(evt.Topic).Match(
            Some: head => Shed(head.Topic, cell.Runtime.Level())
                ? IO.lift(() => fun(cell.Runtime.Drops)(DropReceipt.Shed(head.Topic, head.Cursor.Value)))
                : TopicFabric.Publish(head, evt),
            None: () => IO.fail<Unit>(new BusFault.TopicUnknown(evt.Topic.Key)));

    static bool Shed(Topic topic, DegradationLevel level) =>
        topic.Sheds.Match(Some: threshold => level.Rank >= threshold.Rank, None: static () => false);

    // Conductor tokens bound every await, so the drain-forced escalation reaches an in-flight consumer
    // instead of waiting on a completion nothing cancels.
    static IO<Unit> Drain(TopicHead head, Seq<Subscription> subs, Action<FanMiss> missed, CancellationToken token) =>
        from _fan in Completed(head.Fan, token)
        from _subs in subs.TraverseM(sub => Completed(sub.Consumer, token)).As()
        from _residual in IO.lift(() => subs.Fold(unit, (_, sub) => Residual(head, sub, missed)))
        select unit;

    static IO<Unit> Completed(DrainQueue<DomainEvent> queue, CancellationToken token) =>
        IO.liftAsync(async () => {
            await queue.Drained(token).ConfigureAwait(false);
            return unit;
        });

    // Tail loss closes HERE and nowhere else: a subscription that missed the final offers never sees a later
    // ordinal, so the gap fold alone stays blind to them. Cursor reads AFTER every consumer completed, so the
    // head's ordinal is final and its distance from the seat is exactly what never arrived.
    static Unit Residual(TopicHead head, Subscription sub, Action<FanMiss> missed) =>
        (head.Cursor.Value, sub.Seat.Value.Last) switch {
            var (final, seated) when final > seated => fun(missed)(new FanMiss(TopicFabric.Fan.Name, sub.Key, seated + 1, final)),
            _ => unit,
        };
}
```

```mermaid
---
config:
  layout: elk
  flowchart:
    curve: linear
    padding: 25
---
flowchart LR
    accTitle: One topic fan, one waiting room per subscription, loss receipted at both ends
    accDescr: A DomainEvent enters EventBus.Dispatch from the outbox dispatch sweep or an in-process producer; the degradation shed gate either mints a Shed drop receipt or hands the event to the topic fan, which stamps the topic's next dense offset through the drain-surface broadcast builder; each subscription is one bounded consumer sink seated directly as a fan sink, folding the arriving offset into its seat and minting a Missed drop receipt for any gap before the composed dedupe window admits the event; the bus drains under the runtime conductor, which compares the head cursor against each seat and mints the residual Missed receipt for tail loss.
    Sweep["outbox dispatch sweep"] --> Dispatch
    Producer["in-process producer"] --> Dispatch
    Dispatch["EventBus.Dispatch + shed gate"] --> Fan["DrainSurface.Broadcast (topic fan, offset stamp)"]
    Dispatch -- shed --> Drops["DropReceipt stream"]
    Fan --> Act1["ActionSink + gap fold + DedupeWindow (sub A)"]
    Fan --> Act2["ActionSink + gap fold + DedupeWindow (sub B)"]
    Act1 -- gap --> Drops
    Act2 -- gap --> Drops
    Act1 --> Drain["DrainConductor band"]
    Act2 --> Drain
    Drain -- residual --> Drops
```

## [05]-[TS_PROJECTION]

- Owner: `DropReceiptWire` — the bus's whole loss account riding `ReceiptEnvelopeWire` to the sink; `TopicsMap` the one `[Mapper]` projecting it.
- Packages: Riok.Mapperly, BCL inbox
- Growth: one wire-member row per new drop-receipt field; the loss class crosses as its smart-enum key; zero new surface.
- Boundary: loss conservation is folder law and this is the face that makes it observable — a receipt family accounted in process and reaching no board is a conservation nobody can check, so the drop stream fans under its own `ReceiptKind` exactly as the hop and delivery receipts do; the topic and the loss class cross as their smart-enum keys because a peer holds neither roster; the span crosses inclusive on both ends with `count` DERIVED at the C# owner rather than crossing beside it, since two numbers describing one span drift the moment a decoder computes one of them; `resent` crosses because the board's operator question is whether the outbox will re-send what the fan lost, and it derives from the topic's own durability column at the producer so no decoder re-implements the rule; the subscription crosses ABSENT on a `Shed` row, because the degradation gate refused the dispatch before any fan offered it and naming a subscription there invents a target — absence encodes once for this seam at `Runtime/ports#WIRE_LAW`, so the unset slot is an omitted key and the peer face spells `subscription?: string`.

```csharp signature
// --- [BOUNDARIES] ---------------------------------------------------------------------------
public sealed record DropReceiptWire(
    string Topic, string? Subscription, string Class, ulong First, ulong Last, ulong Count, bool Resent);

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Both,
        EnabledConversions = MappingConversionType.All & ~MappingConversionType.ExplicitCast)]
internal static partial class TopicsMap {
    [MapProperty(nameof(DropReceipt.Topic), nameof(DropReceiptWire.Topic), Use = nameof(TopicKey))]
    [MapProperty(nameof(DropReceipt.Subscription), nameof(DropReceiptWire.Subscription), Use = nameof(SubscriptionText))]
    [MapProperty(nameof(DropReceipt.Class), nameof(DropReceiptWire.Class), Use = nameof(ClassKey))]
    public static partial DropReceiptWire ToWire(DropReceipt receipt);

    [NamedMapping(nameof(TopicKey))]
    private static string TopicKey(Topic topic) => topic.Key;

    [NamedMapping(nameof(ClassKey))]
    private static string ClassKey(DropClass loss) => loss.Key;

    [NamedMapping(nameof(SubscriptionText))]
    private static string? SubscriptionText(Option<string> subscription) =>
        subscription.Match(Some: static held => held, None: static () => null);
}
```

```ts signature
type DropClassKey = "shed" | "missed";

interface DropReceiptWire {
  readonly topic: string;
  readonly subscription?: string;
  readonly class: DropClassKey;
  readonly first: number;
  readonly last: number;
  readonly count: number;
  readonly resent: boolean;
}
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)

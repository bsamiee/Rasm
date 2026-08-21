# [COMPUTE_INGEST]

Rasm.Compute owns the foreign-delivery boundary the suite admits sensor and dictionary material through: the branch-owned MQTT 5.0 and NATS protocol bindings carried whole as `BrokerBinding` rows, one delivery projection and one subscription pump driven off those rows, the `CaptureAdmission` fan that turns ONE admitted delivery into the ephemeral twin lane and the `Runtime/observation` durable lane, and the bSDD REST transport a Bim classification lookup rides. One identity regime holds the page: bytes arriving from a party this process does not control, admitted once into evidence-carrying owners.

`Runtime/channels` owns the gRPC channel mechanics and the `CallSpine` budget this page's REST leg composes; `Runtime/wire` owns the wire contract and the `ParseGuard` inbound-parse policy whose bound this page's broker leg reads; `Runtime/observation` owns the durable sensor lane the fan's second leg reaches. Grammar, roster, format rows, framing, and the decode pair arrive settled from `Rasm/Domain/event`, and BOTH broker bindings are BRANCH-OWNED here because the specification defines them and no admitted package supplies either. Package spine: MQTTnet, NATS.Net, CloudNative.CloudEvents (the envelope type alone), Microsoft.Extensions.Primitives, Thinktecture.Runtime.Extensions, LanguageExt.Core, and NodaTime.

## [01]-[INDEX]

- [02]-[BROKER_INGEST]: `BrokerBinding` rows carry the MQTT 5.0 and NATS protocol bindings whole — prefix, routing coordinate, pushdown, the delivery reader, the subscription opener, and the bridge bound — under ONE decode, ONE pump, and one `Absorb` fold fanning each admitted delivery onto the capture lane and the durable lane.
- [03]-[REST_INGEST]: `BsddTransport` issues the bSDD class GET under the `Runtime/channels#CALL_POLICY` `DeadlineClass.HopTotal` budget, against a composition-supplied base address and an open-resolver contract, and classifies the response status into retriable and terminal arms.

## [02]-[BROKER_INGEST]

- Owner: `BrokerBinding` — the closed `[SmartEnum<string>]` row family carrying each protocol binding this folder owns: content-mode reach, attribute placement and prefix, routing key, filter-pushdown verdict, the `Reader` projecting one dialect message onto the neutral `BrokerDelivery`, the `Subscribe` opener over a `BrokerSource`, and the `Option<LaneBound>` bridge a callback-delivering protocol needs; `BrokerSource` `[Union]` the held-client family, one arm per row; `BrokerDelivery` the dialect-neutral carrier — the parent adoption, the body, the framing, and the carried attribute pairs — every decode reads; `SensorReading<T>` the typed body paired with the envelope that carried it, every causal, lag, sampling, and expiry fact PROJECTING off that envelope's own attributes; `CaptureAdmission` the one admission policy row the capture sink reads, its `Absorb` fold fanning ONE delivery onto the ephemeral twin lane and the durable observation lane; `BrokerIngress` the one row carrying this subscription's span source, span name, and carrier trust class; `BrokerChannels` the decode, the pump, and the `Capture` admit sink closing the loop onto `WorkLane.CaptureIngest`.
- Law: the CNCF MQTT binding package is REFUSED at the branch and this folder owns the binding instead — that package pins MQTTnet 4.x against the estate's 5.x, reads a `PayloadSegment` getter the restored v5 message dropped, and reaches structured mode alone, so composing it compiles and then faults `MissingMethodException` on every delivery while forfeiting binary mode entirely. Refusing a package is never refusing the binding: MQTT 5.0 carries BOTH content modes, its attributes ride User Properties UNPREFIXED (the one binding in the matrix that prefixes nothing, because v5 gives properties their own namespace), and the topic is its routing key.
- Law: NATS is branch-owned for the same reason — the CNCF NATS binding targets net6.0/netstandard2.0 against the retired v1 client while this folder holds the current `NATS.Net` line — so its rows lower onto `NatsHeaders` under the specification's `ce-` prefix (NATS ≥ 2.2 carries headers) with the subject as the routing key.
- Law: one decode and one pump serve BOTH rows, because the only per-dialect variance is the CARRIER READ — which member holds the body, which holds the framing, which holds the attribute pairs, and how the parent adopts. Those four are the row's `Reader` column, so the page's own claim that a third protocol is one `BrokerBinding` row is true rather than aspirational. NAMED LOSS: the dialect message type no longer appears in a public signature, so a caller holding a raw `MqttApplicationMessage` composes the row's reader instead of a named `Mqtt<T>` overload. Witness: the two overloads carried byte-identical decode bodies and the pump pair carried byte-identical fault folds, so a fix landed at one and drifted at the other.
- Law: `recordedtime` is the RECEIVER's stamp and the ingest writes it, so `Recorded - Occurred` measures the queue a sensor's reading waited in; collapsing the pair onto `time` erases the only lag figure this lane can publish, and a reading whose producer stamped `time` after this receiver observed it grades UNMEASURED rather than reporting a negative wait.
- Law: `expirytime` DROPS a stale reading at admission rather than scoring it — a twin surrogate fed a reading whose delivery window closed reports a present state from a past world, which is worse than reporting nothing — so the expiry gate runs before the fan, not inside it, and the drop is receipted rather than silent.
- Law: `sampledrate` declares the producer's head-sampling denominator and the twin weights by it, so a stream publishing one reading in ten contributes ten readings' worth of evidence rather than one; an absent row reads as unsampled, the only honest default, because a producer that samples always says so.
- Law: the inbound body bound is the package's ONE inbound-parse policy. `Runtime/wire#CONTRACT_EVOLUTION` `ParseGuard.Canonical.SizeLimitBytes` is the bound every foreign buffer this package decodes crosses, so a broker body and a gRPC message are refused on one declared ceiling and a second literal here would be a per-dialect ceiling nothing reconciles. NAMED LOSS: `ParseGuard.Read` itself does not compose on this leg — its `where T : IBufferMessage` constraint is protobuf's and a broker body is a CloudEvents envelope — so the policy VALUE crosses and the parse stays the kernel decode's.
- Law: a lane's bound is ROW data. The MQTT bridge takes its `LaneBound` from the row rather than a `capacity` parameter, and its full-mode is `Parked` because the ack rides the successful enqueue: a shedding bridge would ack nothing and the broker's redelivery is the whole recovery. NAMED LOSS: a composition can no longer widen one subscription's bridge without a row edit. Witness: a call-site capacity made the backpressure decision unrecoverable from any declaration, so no conservation audit could walk it.
- Law: subscription QoS is fixed at `AtLeastOnce` by the ack law rather than offered as a parameter. QoS 0 carries no redelivery, which makes `AutoAcknowledge = false` and `ProcessingFailed` pure ceremony, and QoS 2 pays a second round trip for an exactly-once guarantee the expiry gate and the content-keyed chunk already make unnecessary. NAMED LOSS: a deployment wanting fire-and-forget telemetry now states it as a row, not a call.
- Entry: `BrokerChannels.Decode<T>(BrokerIngress ingress, BrokerBinding binding, BrokerDelivery delivery, ClockPolicy clocks, Op key)` is the ONE message-to-reading adapter — it opens the consumer bracket through the delivery's own adoption, takes the STRUCTURED leg through `EventEnvelope.Decode` when the framing names an admitted event format and the BINARY leg through `BrokerCodec.Raise` over the row's prefixed carrier otherwise, stamps `recordedtime`, and projects the typed body. `BrokerChannels.Pump<T>(BrokerIngress ingress, BrokerBinding binding, BrokerSource source, string filter, ClockPolicy clocks, Op key, CancellationToken ct)` is the ONE subscription pump, yielding `IAsyncEnumerable<Fin<SensorReading<T>>>` — a subscribe refusal, a severed session, or an enumeration failure yields one CLASSIFIED terminal fault and ends the stream, and cancellation rethrows. `BrokerChannels.Capture(IAsyncEnumerable<Fin<SensorReading<TwinSignal>>> deliveries, CaptureAdmission admission, CancellationToken ct)` is the sink closing the loop — each delivery folds through `CaptureAdmission.Absorb`, and a refusal on either leg parks on the injected arrow rather than ending the subscription.
- Law: a terminal fault carries the transience its cause has. A broker that refuses a subscription with `NotAuthorized` or `TopicFilterInvalid` is deterministic and lands on an arm inheriting the kernel `Terminal` default, while a quota exceedance, a severed session, a connection failure, and a timeout land on `ComputeFault.EndpointUnreachable`, which PUBLISHES `Retriability.Transient` at its owner. NAMED LOSS: the one-string catch-all was shorter. Witness: it folded a disposed client and an invalid topic filter onto the same transient arm, so the re-drive rail re-attempted a refusal that answers identically forever, and it discarded the SUBACK result entirely, so a `NotAuthorized` grant read as a healthy subscription that simply never delivered.
- Auto: every ingress bracket stamps the `Rasm.AppHost/Wire/companion#EVENT_INGRESS` `EventSemconv` families off the envelope beside the row's own `System` and routing coordinate, so a broker delivery and an HTTP delivery answer one query; content mode resolves from the message ITSELF rather than a per-subscription flag — a framing the kernel format rows admit is structured and anything else is binary — so one publisher switching modes mid-stream costs a consumer nothing and no composition carries a mode knob that can disagree with what a broker delivers. Both legs land the SAME `CloudEvent` under the SAME `EventRoster.Declared`, so a declared extension decodes typed on either path and the lag, expiry, and sampling projections read one attribute space. Wire context absent extracts empty, which the propagator already treats as a root, so neither dialect spells an absent-pair arm. NATS control frames (`NatsMsgFlags` via `IsEmpty`/`HasNoResponders`) resolve inside that row's own reader and never reach the decode.
- Receipt: decoding emits no receipt case; an expired reading mints one `Backpressure`-band drop carrying its own cause, and a lane the AppHost governor has shed mints the same band carrying the `ShedCause` the verdict names, so both refusals are attributable rather than absent. `Capture` admits each typed reading onto `WorkLane.CaptureIngest` through the one `AdmittedIntent` gate; the NATS `queueGroup` load-balances one subject across N capture subscribers.
- Packages: MQTTnet (`IMqttClient.ApplicationMessageReceivedAsync`/`DisconnectedAsync`/`SubscribeAsync`, `MqttClientSubscribeOptionsBuilder.WithTopicFilter`/`Build`, `MqttClientSubscribeResult.Items`, `MqttClientSubscribeResultItem.ResultCode`, `MqttApplicationMessage.Payload`/`UserProperties`/`ContentType`/`Topic`, `MqttApplicationMessageReceivedEventArgs.AutoAcknowledge`/`AcknowledgeAsync`/`ProcessingFailed`, `MqttClientDisconnectedEventArgs.Reason`, `MqttUserProperty.Name`/`ValueBuffer` under `MqttUserPropertyExtensions.ReadValueAsString`), NATS.Net (`INatsClient.SubscribeAsync<byte[]>`, `NatsMsg<byte[]>.Data`/`.Headers`/`.IsEmpty`/`.HasNoResponders`, the self-naming fault family), CloudNative.CloudEvents (`CloudEvent`, `CloudEventAttribute`, `CloudEventsSpecVersion`), LanguageExt.Core, NodaTime, Microsoft.Extensions.Primitives (`StringValues` the `NatsHeaders` value), Rasm (project — the `Rasm/Domain/event` envelope algebra beside the kernel `TraceCarrier`/`SpanEdge` causal band), Rasm.AppHost (project — `TraceContext`/`TenantAdoption`, `Admission`/`LaneReading`/`ShedCause`, `ClockPolicy`), BCL inbox (`System.Buffers.ReadOnlySequence<byte>`, `System.Diagnostics`, `System.Net.Mime`, `System.Threading.Channels`)
- Growth: a new typed broker body reuses `Decode<T>` with its own `T`; a new envelope attribute is one `EventExtension` row at the kernel that both legs decode with no edit here; a new admission stance is one `CaptureAdmission` value and a new ingress trust class one `BrokerIngress` value, never a knob on `Capture`; a second lane consumer earns one `ComputeIntent` case beside `SensorAdmit` and a second delivery CONSEQUENCE one leg on `Absorb`, never a second subscription over the same subject; a third protocol is one `BrokerBinding` row carrying its reader, its opener, and its bridge verdict beside one `BrokerSource` arm — no second decode, no second pump, no second propagator; the request/reply remote-compute RPC leg (`INatsConnection.RequestAsync`/`NatsMsg.ReplyAsync`) rides the same connection beside the fire-and-forget subscription. Provider reconnect mechanics remain provider-owned.
- Boundary: `MqttUserProperty.Value` is `[Obsolete]` at the admitted pin, so the live read is `ValueBuffer` through the package's own `ReadValueAsString` extension and a fence spelling `Value` compiles against a member the distribution already retired. MQTTnet delivers on an EVENT rather than an enumerator, so the MQTT row's opener bridges the client receive loop onto the enumerable shape and `AutoAcknowledge` is FALSE with the ack riding the successful enqueue alone — an auto-acked drop loses the QoS 1/2 delivery redelivery recovers — and the handler detaches on the finally arm so no completed channel keeps a live writer. That bridge is BRANCH-LOCAL and not the AppHost `SubscriptionLane`: that lane's queue is `DrainQueue<ExternalValue>`, a BMS-coerced scalar carrying a `BindingSpec`, a `Quality`, and a `TenantContext`, so routing a delivery through it destroys the envelope this leg exists to decode. Refusal evidence partitions cleanly across four mechanisms and never double-counts one reading: a full bridge sets `ProcessingFailed` and the reading never entered a lane, so the BROKER's own redelivery is its whole evidence; an expired reading is refused at admission and receipted by this leg; a shed lane refuses BEFORE the enqueue on the governor's own verdict and carries its `ShedCause`; a `DropOldest` drop on `WorkLane.CaptureIngest` reached the lane, so a correlated `Backpressure` receipt is ITS whole evidence and no broker redelivery follows an already-acked delivery. Parent adoption is the spine propagator's and parent PROJECTION the kernel's: the row's reader supplies `TraceContext.Continue` — the one seam that adopts the inbound context AND admits the delivery's tenancy under its trust row — `EventEnvelope.Trace` projects the envelope's own creation-time pair, and `SpanEdge.Under` is the consuming bracket, so a literal `traceparent`/`tracestate` pair read at either dialect, a hand-built carrier record, and an ingress that adopts a trace while dropping its tenant are three forms of one defect and none survives here. NATS holds one long-lived per-instance `INatsClient`/`NatsConnection` shared across subjects, never one connection per subscription and never a process-global static; JetStream/KV/Object surfaces are the Persistence `api-nats` overlay's; grammar, roster, format rows, framing, and the decode pair are `Rasm/Domain/event`'s whole, so this page holds no formatter instance, no media-type literal, and no attribute-name literal outside the binding prefixes the specification itself fixes; `dataref` residence, threshold, retention, and dual-shipping stay unbound because a sensor reading is small by construction and a body this lane cannot resolve refuses rather than externalizing.

| [INDEX] | [BINDING] | [MODES]            | [PLACEMENT]                 | [ROUTES_ON] | [PUSHDOWN]                     | [BRIDGE]                |
| :-----: | :-------- | :----------------- | :-------------------------- | :---------- | :----------------------------- | :---------------------- |
|  [01]   | `mqtt`    | binary, structured | User Properties, UNPREFIXED | topic       | broker SUBSCRIBE topic filters | parked, callback source |
|  [02]   | `nats`    | binary, structured | headers, `ce-` prefixed     | subject     | broker subject wildcards       | none, enumerator source |

```csharp signature
// --- [TYPES] ----------------------------------------------------------------------------
// The held client per row. A subscription needs the connection AND whatever configures it for this protocol, so
// the NATS queue group rides the arm holding the client rather than a parameter tail on a pump that would then
// carry an MQTT-meaningless argument.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record BrokerSource {
    private BrokerSource() { }

    public sealed record Mqtt(IMqttClient Client) : BrokerSource;
    public sealed record Nats(INatsClient Client, Option<string> QueueGroup) : BrokerSource;
}

// --- [MODELS] ---------------------------------------------------------------------------
// The dialect-neutral delivery every decode reads. `Adopt` is the propagation continuation the DIALECT supplies
// — MQTT through the propagation owner's own message overload, NATS through the generic overload and one header
// getter — so the decode names no carrier field and a field landing at that owner reaches both rows unedited.
public sealed record BrokerDelivery(
    Func<BrokerIngress, IDisposable> Adopt,
    ReadOnlyMemory<byte> Body,
    Option<ContentType> Framing,
    Seq<(string Name, string Value)> Carried);

// Typed body beside the envelope that carried it. Causality, ingest instant, sampling denominator, and expiry all
// PROJECT off the envelope's own rostered attributes rather than sitting beside it as fields a decode could fill
// inconsistently. Lag answers None where the producer stamped an instant after this receiver observed it, since a
// negative wait is unmeasured rather than zero.
public sealed record SensorReading<T>(CloudEvent Envelope, T Data) {
    public TraceCarrier Trace => EventEnvelope.Trace(Envelope);

    public Option<Instant> Occurred => Optional(Envelope.Time).Map(static at => Instant.FromDateTimeOffset(at));

    public Option<Instant> Recorded => Read<DateTimeOffset>(EventExtension.RecordedTime).Map(Instant.FromDateTimeOffset);

    public Option<Duration> Lag =>
        from recorded in Recorded
        from occurred in Occurred
        where recorded >= occurred
        select recorded - occurred;

    // Head sampling declares a DENOMINATOR, so an absent row reads as unsampled — the only honest default,
    // because a producer that thins its stream is the party that knows it did.
    public int Sampled => Read<int>(EventExtension.SampledRate).Filter(static rate => rate > 0).IfNone(1);

    public bool Expired(Instant now) =>
        Read<DateTimeOffset>(EventExtension.ExpiryTime).Map(Instant.FromDateTimeOffset).Match(
            Some: expiry => now > expiry,
            None: static () => false);

    private Option<TValue> Read<TValue>(EventExtension row) =>
        row.Read<TValue>(Envelope, Op.Of(name: nameof(SensorReading<T>))).ToOption().Flatten();
}

// Every dialect adapter takes this ingress row, so causality and trust arrive as ONE value rather than a
// three-argument tail per pump. `Adoption` carries no default because trust is a property of the transport a
// composition owns — a broker on the estate's own bus adopts its wire tenancy, a public endpoint refuses it —
// and a defaulted arm hands every later dialect whichever answer read safer the day it was written.
public sealed record BrokerIngress(ActivitySource Source, TenantAdoption Adoption, string Span);

// --- [TABLES] ---------------------------------------------------------------------------
// Binding rows carry the whole protocol variation the specification defines and no package here supplies: which
// content modes the protocol reaches, where attributes ride, whether the placement prefixes, what the protocol
// routes on, whether a subscription filter resolves at the broker, how one delivery projects onto the neutral
// carrier, how a subscription opens, and whether the delivery shape needs a bridge at all.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class BrokerBinding {
    // MQTT 5.0 gives User Properties their own namespace, so the specification prefixes NOTHING here — the one
    // binding in the matrix that does not, and the exact fact a prefix-assuming reader gets silently wrong.
    public static readonly BrokerBinding Mqtt = new("mqtt",
        prefix: "", routes: "topic", pushdown: true, system: "mqtt",
        bridge: Some(new LaneBound.Parked(1024)),
        subscribe: MqttBinding.Subscribe);

    public static readonly BrokerBinding Nats = new("nats",
        prefix: "ce-", routes: "subject", pushdown: true, system: "nats",
        bridge: Option<LaneBound>.None,
        subscribe: NatsBinding.Subscribe);

    public string Prefix { get; }

    // Ingress spans stamp this `messaging.system` value beside the envelope's own `cloudevents.*` five, so a
    // broker delivery here and an HTTP delivery at `Rasm.AppHost/Wire/companion#EVENT_INGRESS` join one query.
    public string System { get; }

    // What the protocol partitions and filters on, so a `partitionkey` extension lowers onto the row's own
    // coordinate rather than a per-leg guess.
    public string Routes { get; }

    // Both protocols resolve a subscription's topic or subject filter AT THE BROKER, so a filter dialect keyed
    // on the routing coordinate never reaches a consumer-side fold on these rows.
    public bool Pushdown { get; }

    // A callback-delivering protocol owes a bridge and an enumerator-delivering one owes none, so the column is
    // Option-shaped: an unread `LaneBound` on the NATS row would be a declared bound nothing enforces.
    public Option<LaneBound> Bridge { get; }

    public Func<BrokerSource, string, Option<LaneBound>, CancellationToken, IAsyncEnumerable<Fin<BrokerDelivery>>> Subscribe { get; }

    // Attribute names cross the wire under this row's prefix in BOTH directions, so a name a decode reads and a
    // name an encode wrote cannot disagree about whether the prefix is part of the name.
    public string Wire(string attribute) => Prefix + attribute;

    public Option<string> Attribute(string carried) =>
        carried.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase) ? Some(carried[Prefix.Length..]) : None;
}

// --- [BOUNDARIES] -----------------------------------------------------------------------
// Admission policy for the capture sink — one row, never a parameter ladder at the fold. The composition supplies
// the lane runtime, the governor's own verdict read, the intent policy, the correlation mint, the parent cancel
// scope, the clock triple, the durable lane, and the refusal arrow; that sink seats `WorkLane.CaptureIngest`
// itself so no composition can route sensor pressure onto a lane that starves interactive work. `Observations` is
// `Option` because a composition running the twin alone is a real deployment — a scoring loop over a model
// carrying no instrumented occurrences has nothing to write back — while a defaulted lane would silently
// accumulate against an empty roster.
public sealed record CaptureAdmission(
    LaneRuntime Lanes,
    Func<WorkLane, Admission> Governor,
    Spec Spec,
    Func<SensorReading<TwinSignal>, CorrelationId> Correlate,
    CancelScope Scope,
    ClockPolicy Clocks,
    Option<ObservationLane> Observations,
    Func<Error, IO<Unit>> Refused) {
    public Fin<AdmittedIntent> Admit(SensorReading<TwinSignal> reading) =>
        AdmittedIntent.Admit(
            new ComputeIntent.SensorAdmit(reading),
            Spec with { Lane = WorkLane.CaptureIngest },
            Correlate(reading),
            Scope,
            Clocks);

    // ONE delivery, TWO consequences — the ephemeral twin admit and the durable observation accumulate — fanned
    // HERE rather than at a second subscription: a parallel subscribe pays the wire cost twice and, under a NATS
    // queue group, hands the two legs DIFFERENT samples, so the durable record and the scored window drift apart
    // for exactly the readings a rebalance moved. Each leg's refusal parks on the arrow independently.
    //
    // TWO gates run ahead of both legs and each answers a different question. EXPIRY asks whether the reading is
    // still true: one whose delivery window closed would have the twin report a present state from a past world.
    // The GOVERNOR asks whether the lane can take it at all: reading the in-process verdict here carries the
    // lane, the degradation level, and the shed CAUSE into the refusal, where enqueueing into a dark or broken
    // lane surfaced later as an untagged drop no operator could attribute.
    public IO<Unit> Absorb(SensorReading<TwinSignal> reading) =>
        reading.Expired(Clocks.Now)
            ? Refused(new ComputeFault.PayloadOverBounds($"<broker-reading-expired:{reading.Envelope.Id}>"))
            : Governor(WorkLane.CaptureIngest) is Admission.ShedCase shed
                ? Refused(new ComputeFault.PayloadOverBounds($"<capture-lane-shed:{shed.Cause.Key}:{shed.Reading.Level.Key}>"))
                : Admit(reading)
                    .Match(Succ: intent => Lanes.Enqueue(intent).Map(static _ => unit), Fail: Refused)
                    .Bind(_ => Observations.Match(
                        Some: lane => lane.Admit(reading).Bind(landed =>
                            landed.Match(Succ: static _ => IO.pure(unit), Fail: Refused)),
                        None: static () => IO.pure(unit)));
}

// --- [OPERATIONS] -----------------------------------------------------------------------
public static class BrokerChannels {
    // BOTH protocol bindings live here, branch-owned, because the specification defines them and the estate
    // admits no package for either. Lower and raise are one pair per row: `Raise` rebuilds the envelope from a
    // binary-mode carrier the row's own prefix names, and the structured leg needs no pair at all because the
    // whole envelope is the body the kernel decode already owns.
    public static class BrokerCodec {
        // Structured mode is whatever the framing SAYS: a content type the kernel format rows admit carries a
        // whole envelope, and anything else is a binary-mode body whose attributes ride the carrier. Reading that
        // framing off the message rather than off a subscription flag lets one publisher switch modes mid-stream
        // with no consumer edit, and stops a composition asserting a mode the broker contradicts.
        public static Fin<CloudEvent> Structured(ReadOnlyMemory<byte> body, ContentType framing, Op key) =>
            EventEnvelope.Decode(new EventFrame(Body: body, Framing: framing), key)
                .Bind(rows => rows is [CloudEvent single]
                    ? Fin.Succ(single)
                    : Fin.Fail<CloudEvent>(new ComputeFault.WireDecodeRejected($"<broker-batch-on-stream:{rows.Count}>")));

        // BINARY mode: attributes ride the transport carrier under the row's prefix and the body is the data
        // alone. UN-PREFIXING is this binding's whole contribution — the row's own `Attribute` strips the dialect
        // and a name the dialect never carried drops here — and the un-prefixed pairs cross to the kernel's
        // `EventEnvelope.Raise`, the declared inverse of its mint. Rebuilding an envelope beside that funnel is a
        // SECOND construction site inside one branch.
        public static Fin<CloudEvent> Raise(
            BrokerBinding binding, Seq<(string Name, string Value)> carried, ReadOnlyMemory<byte> body,
            Option<ContentType> dataType, Op key) =>
            EventEnvelope.Raise(
                attributes: carried.Choose(row => binding.Attribute(row.Name).Map(name => (Name: name, Value: row.Value))),
                data: body, dataType: dataType, key: key);
    }

    // ONE adapter over BOTH branch-owned bindings. Causality enters through the delivery's own adoption, which is
    // the ONE propagation owner driving the composite propagator over the dialect's carrier, so this fold spells
    // no `traceparent` literal, no `tracestate` twin, and no per-property reader. That adoption is also the one
    // seam that ADMITS the delivery's tenancy under its `TenantAdoption` row — a bare pair read adopted nothing,
    // so every receipt, meter tag, and RLS predicate downstream answered root for a delivery that named a tenant.
    // The bracket spans the decode alone; the reading projects its creation-time carrier off the envelope and the
    // lane's own admit bracket descends from the continued span.
    // Exemption: the `using` bracket is the platform-forced boundary seam the subscription law names.
    public static Fin<SensorReading<T>> Decode<T>(
        BrokerIngress ingress, BrokerBinding binding, BrokerDelivery delivery, ClockPolicy clocks, Op key) {
        using IDisposable adopted = delivery.Adopt(ingress);
        return Bounded(delivery.Body, key)
            .Bind(body => delivery.Framing.Filter(static type => EventFormat.Of(type).IsSome).Match(
                Some: type => BrokerCodec.Structured(body, type, key),
                None: () => BrokerCodec.Raise(binding, delivery.Carried, body, delivery.Framing, key)))
            .Bind(envelope => Received(envelope, clocks, key))
            .Bind(Project<T>);
    }

    // ONE pump over both rows: the row's opener answers the enumerable and this fold owns only the decode, so a
    // third protocol adds an opener rather than a second copy of this loop.
    // Exemption: the `await foreach` drain is the platform-forced statement seam the subscription law names.
    public static async IAsyncEnumerable<Fin<SensorReading<T>>> Pump<T>(
        BrokerIngress ingress,
        BrokerBinding binding,
        BrokerSource source,
        string filter,
        ClockPolicy clocks,
        Op key,
        [EnumeratorCancellation] CancellationToken ct = default) {
        await foreach (Fin<BrokerDelivery> delivery in binding.Subscribe(source, filter, binding.Bridge, ct).WithCancellation(ct).ConfigureAwait(false)) {
            yield return delivery.Bind(admitted => Decode<T>(ingress, binding, admitted, clocks, key));
        }
    }

    // The package's ONE inbound-parse bound, read from the policy owner rather than re-declared: a broker with no
    // message-size limit of its own cannot make this process allocate past the ceiling the wire already declares.
    private static Fin<ReadOnlyMemory<byte>> Bounded(ReadOnlyMemory<byte> body, Op key) =>
        body.Length <= ParseGuard.Canonical.SizeLimitBytes
            ? Fin.Succ(body)
            : Fin.Fail<ReadOnlyMemory<byte>>(new ComputeFault.PayloadOverBounds(
                $"<broker-body-over-bound:{body.Length}:{ParseGuard.Canonical.SizeLimitBytes}>"));

    // Ingest writes the RECEIVER's own stamp once and no producer ever supplies it: `recordedtime` beside `time`
    // is what makes queue wait measurable at all, and stamping it anywhere later measures this process's
    // scheduling rather than the broker's hold. A producer that already claimed the slot keeps its value, since a
    // relay re-stamping an upstream receiver's ingest erases the hop the pair was recording.
    private static Fin<CloudEvent> Received(CloudEvent envelope, ClockPolicy clocks, Op key) =>
        EventExtension.RecordedTime.Read<DateTimeOffset>(envelope, key).Bind(held => held.Match(
            Some: _ => Fin.Succ(envelope),
            None: () => EventExtension.RecordedTime.Write(envelope, clocks.Now.ToDateTimeOffset(), key)));

    // Typed projection is the whole reason this lane holds a generic reading: an untyped `CloudEvent.Data` recast
    // at the twin lets a malformed body reach a scoring loop as a cast fault rather than a refusal.
    private static Fin<SensorReading<T>> Project<T>(CloudEvent envelope) =>
        envelope.Data is T data
            ? Fin.Succ(new SensorReading<T>(envelope, data))
            : Fin.Fail<SensorReading<T>>(new ComputeFault.WireDecodeRejected($"<broker-reading-data:{envelope.Id}>"));

    // ADMIT SINK closing the sensor loop: every decoded delivery enters `CaptureAdmission.Absorb`, whose gate
    // lands it on the CaptureIngest channel as a `ComputeIntent.SensorAdmit` — so deadline, element cap, cancel
    // scope, and correlation bind before the lane holds it — and whose durable leg accumulates the same reading
    // toward its content-keyed chunk. A refused DECODE parks here and every refusal INSIDE the fan parks there,
    // so one malformed publisher costs one sample and never the subscription; `LaneRuntime` owns the dispatch
    // delegate, so `TwinLoop.Ingest` binds at composition and this fold names no scoring surface. The drain is
    // single-consumer per stream, which is also what makes the durable lane's window hand-off exclusive without a
    // second cell.
    // Exemption: the `await foreach` drain is the platform-forced statement seam the subscription law names.
    public static IO<Unit> Capture(
        IAsyncEnumerable<Fin<SensorReading<TwinSignal>>> deliveries,
        CaptureAdmission admission,
        CancellationToken ct) =>
        IO.liftAsync(async env => {
            await foreach (Fin<SensorReading<TwinSignal>> delivery in deliveries.WithCancellation(ct).ConfigureAwait(false)) {
                await delivery.Match(Succ: admission.Absorb, Fail: admission.Refused).RunAsync(env).ConfigureAwait(false);
            }
            return unit;
        });
}

// --- [SUBSCRIBE_MQTT]
// MQTTnet delivers on an EVENT rather than an enumerator, so one bounded bridge carries the client's receive loop
// onto the enumerable shape the pump consumes. `AutoAcknowledge` is FALSE and the ack rides the successful
// enqueue alone, so a bridge the lane cannot drain leaves QoS 1/2 deliveries unacked and the broker redelivers
// them. The finally arm detaches BOTH handlers through `-=` against the same handles `+=` bound, since an event
// left subscribed past the pump holds the closure and writes into a completed channel for the client's lifetime.
internal static class MqttBinding {
    // Both terminal paths — the SUBACK refusal and the severed session — settle ONE cell, so the reader drains
    // what already arrived and then yields exactly one classified fault. The cell is an `Atom` rather than a
    // captured `Error?`: two closures and a loop write it, and a nullable local under that access pattern is the
    // read-modify-write outside a CAS the rail owner deletes.
    // Exemption: the callback attach/detach pair and the iterator try/catch are the platform-forced seams.
    public static async IAsyncEnumerable<Fin<BrokerDelivery>> Subscribe(
        BrokerSource source, string topicFilter, Option<LaneBound> bound,
        [EnumeratorCancellation] CancellationToken ct = default) {
        if (source is not BrokerSource.Mqtt { Client: { } client }) {
            yield return Fin.Fail<BrokerDelivery>(new ComputeFault.Violation(ComputeArea.Runtime, new ComputeViolation.Contract(ComputeContract.Compatible, new ContractEvidence.Type(source.GetType()))));
            yield break;
        }

        Atom<Option<Error>> terminal = Atom(Option<Error>.None);
        if (Bridged(bound).Case is not Channel<MqttApplicationMessage> bridge) {
            yield return Fin.Fail<BrokerDelivery>(new ComputeFault.LaneUnprofiled($"<mqtt-bridge-unranked:{topicFilter}>"));
            yield break;
        }

        async Task Deliver(MqttApplicationMessageReceivedEventArgs delivery) {
            delivery.AutoAcknowledge = false;
            if (bridge.Writer.TryWrite(delivery.ApplicationMessage)) { await delivery.AcknowledgeAsync(ct).ConfigureAwait(false); }
            else { delivery.ProcessingFailed = true; }
        }

        // A severed session stops delivery WITHOUT completing the bridge, so the drain would wait on a client
        // that will never write again — the one failure a subscription reports as silence. A disconnect is a
        // transport fact, so it lands the arm that publishes `Transient` and the re-drive rail may re-attempt it.
        Task Severed(MqttClientDisconnectedEventArgs ended) {
            terminal.Swap(held => held.IsSome
                ? held
                : Some<Error>(new ComputeFault.EndpointUnreachable($"<mqtt-disconnected:{topicFilter}:{ended.Reason}>")));
            _ = bridge.Writer.TryComplete();
            return Task.CompletedTask;
        }

        client.ApplicationMessageReceivedAsync += Deliver;
        client.DisconnectedAsync += Severed;
        try {
            Option<Error> granted = await Granted(client, topicFilter, ct).ConfigureAwait(false);
            terminal.Swap(held => held.IsSome ? held : granted);
            if (terminal.Value.Case is Error refused) { yield return Fin.Fail<BrokerDelivery>(refused); yield break; }
            await foreach (MqttApplicationMessage message in bridge.Reader.ReadAllAsync(ct).ConfigureAwait(false)) {
                yield return Fin.Succ(Read(message));
            }
            if (terminal.Value.Case is Error severed) { yield return Fin.Fail<BrokerDelivery>(severed); }
        }
        finally {
            client.ApplicationMessageReceivedAsync -= Deliver;
            client.DisconnectedAsync -= Severed;
            _ = bridge.Writer.TryComplete();
        }
    }

    // The SUBACK is DATA the package returns, not an exception it raises: MQTTnet throws only for LOCAL faults —
    // a tripped token, an invalid topic, a disposed or unconnected client, a feature the validator refuses — and
    // every broker verdict rides a per-filter reason code. Discarding the result left a `NotAuthorized` grant
    // reading as a healthy subscription that simply never delivered, and folding the local throw onto the
    // transient arm re-drove a disposed client forever.
    private static async Task<Option<Error>> Granted(IMqttClient client, string topicFilter, CancellationToken ct) =>
        (await Op.Of(name: "mqtt-subscribe").Catch(
            async _ => Fin.Succ(await client.SubscribeAsync(
                new MqttClientSubscribeOptionsBuilder()
                    .WithTopicFilter(topicFilter, MqttQualityOfServiceLevel.AtLeastOnce)
                    .Build(),
                ct).ConfigureAwait(false)),
            ct).ConfigureAwait(false))
        .Match(
            Succ: result => toSeq(result.Items)
                .Find(static item => item.ResultCode >= MqttClientSubscribeResultCode.UnspecifiedError)
                .Map(item => Refusal(topicFilter, item.ResultCode)),
            Fail: Some);

    // Quota is a capacity fact the broker recovers from and everything else on this lane is a standing refusal,
    // so the transience is decided by the reason code rather than by which arm the author reached first.
    private static Error Refusal(string topicFilter, MqttClientSubscribeResultCode code) =>
        code is MqttClientSubscribeResultCode.QuotaExceeded
            ? new ComputeFault.EndpointUnreachable($"<mqtt-subscribe:{topicFilter}:{code}>")
            : new ComputeFault.Violation(ComputeArea.Runtime, new ComputeViolation.Contract(ComputeContract.Valid, new ContractEvidence.Status((int)code)));

    // The bridge COMPOSES the lane family's own generic construction (`Runtime/scheduling#LANE_AXIS`
    // `LaneChannels.Open<T>`) instead of re-switching the union — the re-switch here read `Ranked.Capacity`, a
    // slot that case never carried. A shed delivery stays deliberately unobserved at the drop hook: the unacked
    // QoS 1/2 message redelivers by this page's own ack law, so the loss class is the broker's to replay.
    private static Fin<Channel<MqttApplicationMessage>> Bridged(Option<LaneBound> bound) =>
        bound.Match(
            Some: static row => row.Open(new LaneChannel<MqttApplicationMessage>(
                Readers: 1, InlineContinuations: false, Dropped: static (_, _) => { }, Rank: None)),
            None: static () => Fin.Succ(Channel.CreateUnbounded<MqttApplicationMessage>(new UnboundedChannelOptions { SingleReader = true })));

    // Payload reads single-segment straight through and only a segmented body pays one copy.
    private static BrokerDelivery Read(MqttApplicationMessage message) {
        ReadOnlySequence<byte> payload = message.Payload;
        return new BrokerDelivery(
            Adopt: ingress => TraceContext.Continue(ingress.Source, message, ingress.Span, ingress.Adoption),
            Body: payload.IsSingleSegment ? payload.First : payload.ToArray(),
            Framing: Framing(message.ContentType),
            Carried: toSeq(message.UserProperties ?? []).Map(static row => (row.Name, row.ReadValueAsString())));
    }

    private static Option<ContentType> Framing(string? media) =>
        Optional(media).Filter(static text => text.Length > 0).Map(static text => new ContentType(text));
}

// --- [SUBSCRIBE_NATS]
// NATS Core delivers on an enumerator, so this row opens no bridge: the client drains `SubscribeAsync<byte[]>`
// until the token trips, and control frames resolve here rather than reaching a decode that would report a
// protocol frame as a payload fault.
internal static class NatsBinding {
    // The enumerator runs under the supplied token; `Op.Catch` preserves every foreign failure as its original
    // `Error` while retaining cancellation provenance from that exact execution token.
    public static async IAsyncEnumerable<Fin<BrokerDelivery>> Subscribe(
        BrokerSource source, string subject, Option<LaneBound> bound,
        [EnumeratorCancellation] CancellationToken ct = default) {
        if (source is not BrokerSource.Nats { Client: { } client, QueueGroup: { } queueGroup }) {
            yield return Fin.Fail<BrokerDelivery>(new ComputeFault.Violation(ComputeArea.Runtime, new ComputeViolation.Contract(ComputeContract.Compatible, new ContractEvidence.Type(source.GetType()))));
            yield break;
        }

        await using IAsyncEnumerator<NatsMsg<byte[]>> pump =
            client.SubscribeAsync<byte[]>(subject, queueGroup.IfNoneUnsafe(() => null), cancellationToken: ct).GetAsyncEnumerator(ct);
        while (true) {
            Fin<bool> advanced = await Op.Of(name: "nats-subscribe-next").Catch(
                async _ => Fin.Succ(await pump.MoveNextAsync().ConfigureAwait(false)),
                ct).ConfigureAwait(false);

            if (advanced.Case is Error refused) { yield return Fin.Fail<BrokerDelivery>(refused); yield break; }
            if (advanced.Case is false) { yield break; }
            // The NatsMsgFlags bits mark protocol frames, not payloads, so they resolve before any decode runs.
            if (pump.Current.IsEmpty || pump.Current.HasNoResponders) { continue; }
            yield return Fin.Succ(Read(pump.Current, subject));
        }
    }

    private static BrokerDelivery Read(NatsMsg<byte[]> message, string subject) =>
        new(Adopt: ingress => TraceContext.Continue(
                ingress.Source, message.Headers, Carrier, ingress.Span, ingress.Adoption, ActivityKind.Consumer),
            Body: message.Data ?? ReadOnlyMemory<byte>.Empty,
            Framing: Optional(Carrier(message.Headers, BrokerBinding.Nats.Wire("datacontenttype")).FirstOrDefault())
                .Filter(static text => text.Length > 0).Map(static text => new ContentType(text)),
            Carried: message.Headers is null
                ? Seq<(string, string)>()
                : toSeq(message.Headers).Map(static row => (row.Key, row.Value.ToString())));

    // ONE non-throwing header read serves the propagation adapter, the framing probe, and the carried pairs — the
    // prior `string?`/`IEnumerable<string>` pair differed only in return shape and drifted the day one of them
    // gained the empty-value guard. `NatsMsg.Headers` is `IDictionary<string, StringValues>` with a non-throwing
    // `TryGetValue`; a null map and an empty value both answer the empty extraction the propagator treats as a
    // root, so the absent verdict has one spelling.
    private static IEnumerable<string> Carrier(NatsHeaders? carrier, string key) =>
        carrier is not null && carrier.TryGetValue(key, out StringValues values) && !StringValues.IsNullOrEmpty(values)
            ? [values.ToString()]
            : [];
}
```

## [03]-[REST_INGEST]

- Owner: `BsddTransport` — the buildingSMART Data Dictionary class-lookup transport, distinct from the gRPC axis but riding the same `DeadlineClass.HopTotal` budget through `Runtime/channels#CALL_POLICY` `CallSpine.AwaitedHttp`; `BsddWire` the open-resolver serializer contract its consumer-typed generic demands.
- Entry: `Fetch<TResponse>(string classUri, CancellationToken token)` issues the class GET and deserializes onto a caller-supplied response shape, staying response-DTO-agnostic — the generic names no AEC-domain type — while the Bim `Semantics/classification#BSDD_RESOLUTION` `BsddPort`/`BsddClass.Of` owns the wire DTO, the `LocalShape` degrade, and the projection.
- Law: the endpoint is a COMPOSITION value, never a fence literal. The base address arrives on the injected `HttpClient`, so a mirror, a proxy, an air-gapped cache, and a test double are composition choices rather than edits here, and a design page carries no URL.
- Law: a status is a DISCRIMINANT, not a slug. A 429 carrying a delta `Retry-After` lands `ComputeFault.EndpointThrottled`, whose `Retriability.Throttled(RetryAfter)` carries the server-declared window onto the re-drive rail; a window-less rate limit, a timeout, and a server outage recover on a later attempt and land `ComputeFault.EndpointUnreachable` (`Transient`); a not-found, an unauthorized, and a malformed request answer identically forever and land arms inheriting the kernel `Terminal` default. Witness: every non-2xx previously folded onto one transient arm, so a 404 for an unknown class re-drove until the budget was spent and a throttled peer was re-driven on the rail's own curve rather than the window it named.
- Law: a consumer-typed generic mounts an OPEN resolver. `TypeInfoResolver = Context.Default` resolves only the roots that context registered, and `Fetch<TResponse>` is by construction handed shapes this package never sees, so the options instance declares the reflection-backed resolver EXPLICITLY rather than inheriting it as the silent fallback a source-generated contract claims it deleted.
- Receipt: the REST hop's route, status, and deadline outcome emit through `ReceiptSinkPort.Send` at the `AwaitedHttp` seam, the same seam the gRPC edge reports through.
- Packages: LanguageExt.Core, NodaTime (`Duration` on the throttle arm), Rasm.AppHost (project — `DeadlineClass`), BCL inbox (`System.Net.Http`, `System.Text.Json`, `System.Text.Json.Serialization.Metadata`)
- Growth: a new dictionary query is one `Fetch<TResponse>` call with its own response shape; a new status classification is one arm on the status fold; zero new surface.
- Boundary: a transport miss returns the typed fault the app-root `BsddPort` adapter degrades on, and the app composition root that references both packages closes `Fetch<BsddClassResponse>` and adapts it into the Bim `BsddPort` so neither package depends on the other — a Bim-minted bSDD transport, a Compute-side bSDD response record or local fallback, and a direct cross-package reference in either direction are the rejected forms. A 200 carrying an empty body is a CONTENT refusal and rails as one: the prior `?? throw` was re-caught two frames up and re-labelled unreachable, so an endpoint answering correctly with nothing read as an endpoint that could not be reached.

```csharp signature
// --- [BOUNDARIES] -----------------------------------------------------------------------
public sealed class BsddTransport(HttpClient client, CallSpine spine) {
    // The resolver is DECLARED because the generic is consumer-typed: a `TypeInfoResolver` left unset resolves
    // through reflection anyway, so the contract's silence and its behaviour disagree the moment a sibling seam
    // mounts a source-generated context and a reader assumes both did.
    private static readonly JsonSerializerOptions BsddWire = new(JsonSerializerDefaults.Web) {
        PropertyNameCaseInsensitive = true,
        TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
    };

    public IO<Fin<TResponse>> Fetch<TResponse>(string classUri, CancellationToken token) =>
        spine.AwaitedHttp(classUri, token, async (uri, scope) => {
            // An unseated base address is a COMPOSITION fault, so it rails at the boundary that reads it rather
            // than raising past a rail every other refusal on this leg already crosses.
            if (Optional(client.BaseAddress) is not { Case: Uri seat }) {
                return Fin.Fail<TResponse>(new ComputeFault.Violation(ComputeArea.Runtime, new ComputeViolation.Required(ComputeSubject.Resource)));
            }
            using HttpRequestMessage request = new(HttpMethod.Get,
                new UriBuilder(seat) { Query = $"Uri={Uri.EscapeDataString(uri)}&IncludeClassProperties=true" }.Uri);
            using HttpResponseMessage response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, scope).ConfigureAwait(false);
            return response.IsSuccessStatusCode
                ? Optional(await JsonSerializer.DeserializeAsync<TResponse>(
                        await response.Content.ReadAsStreamAsync(scope).ConfigureAwait(false), BsddWire, scope).ConfigureAwait(false))
                    .ToFin(new ComputeFault.WireDecodeRejected($"<bsdd-empty-body:{uri}>"))
                : Fin.Fail<TResponse>(Refusal(uri, response.StatusCode, Optional(response.Headers.RetryAfter?.Delta)));
        });

    // The date form of `Retry-After` deliberately degrades to the transient arm: only a server-DECLARED delta
    // is a window worth honoring over the rail's own curve, and clock-skewed absolute dates forge negative waits.
    private static Error Refusal(string uri, HttpStatusCode status, Option<TimeSpan> retryAfter) =>
        status is HttpStatusCode.TooManyRequests && retryAfter.Case is TimeSpan window
            ? new ComputeFault.EndpointThrottled($"<bsdd:429:{uri}>", Duration.FromTimeSpan(window))
            : status is HttpStatusCode.TooManyRequests or HttpStatusCode.RequestTimeout or >= HttpStatusCode.InternalServerError
                ? new ComputeFault.EndpointUnreachable($"<bsdd:{(int)status}:{uri}>")
                : new ComputeFault.Violation(ComputeArea.Runtime, new ComputeViolation.Contract(ComputeContract.Valid, new ContractEvidence.Status((int)status)));
}

// The transport-ingest lane's two roster rows ([FaultCase] 27/28), declared here because this lane's folds
// raise them. WireDecodeRejected is the foreign-body DECODE refusal — deterministic, kernel Terminal — distinct
// from PayloadOverBounds (a size fact) and from the symbolic lane's 2212 ParseRejected (a declined
// Entity.TryParse). EndpointThrottled is the ONLY arm publishing the kernel Throttled posture: the
// server-declared window IS the payload, so the re-drive rail waits the window the peer named, never its own
// curve.
public abstract partial record ComputeFault {
    [FaultCase(27)] public sealed partial record WireDecodeRejected(string Detail) : ComputeFault(Detail);

    [FaultCase(28)] public sealed partial record EndpointThrottled(string Detail, Duration RetryAfter) : ComputeFault(Detail) {
        public override Retriability Retriability => Retriability.Throttled(RetryAfter);
    }
}
```

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)

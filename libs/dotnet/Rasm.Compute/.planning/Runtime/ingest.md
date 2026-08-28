# [COMPUTE_INGEST]

Rasm.Compute owns the foreign-delivery boundary the suite admits sensor and dictionary material through: the branch-owned MQTT 5.0 and NATS protocol bindings carried whole as `BrokerBinding` rows, one delivery projection and one subscription pump driven off those rows, the `CaptureAdmission` fan that turns ONE admitted delivery into the ephemeral twin lane and the `Runtime/observation` durable lane, and the bSDD REST transport a Bim classification lookup rides. One identity regime holds the page: bytes arriving from a party this process does not control, admitted once into evidence-carrying owners.

`Runtime/channels` owns the gRPC channel mechanics and the `CallSpine` budget this page's REST leg composes; `Runtime/wire` owns the wire contract and `Runtime/channels` the `WireLimits` inbound ceiling this page's broker leg reads; `Runtime/observation` owns the durable sensor lane the fan's second leg reaches. Grammar, roster, format rows, framing, and the decode pair arrive settled from `Rasm/Domain/event`, and BOTH broker bindings are BRANCH-OWNED here because the specification defines them and no admitted package supplies either. Package spine: MQTTnet, NATS.Net, CloudNative.CloudEvents (the envelope type alone), Microsoft.Extensions.Primitives, Thinktecture.Runtime.Extensions, LanguageExt.Core, and NodaTime.

## [01]-[INDEX]

- [02]-[BROKER_INGEST]: `BrokerBinding` rows carry the MQTT 5.0 and NATS protocol bindings whole — prefix, routing coordinate, pushdown, the delivery reader, the subscription opener, and the queue bound each declares — under ONE decode, ONE pump, and one `Absorb` fold fanning each admitted delivery onto the capture lane and the durable lane.
- [03]-[REST_INGEST]: `BsddTransport` issues the bSDD class GET under the `Runtime/channels#CALL_POLICY` `DeadlineClass.HopTotal` budget, against a composition-supplied base address and an open-resolver contract, and classifies the response status into retriable and terminal arms.

## [02]-[BROKER_INGEST]

- Owner: `BrokerBinding` — the closed `[SmartEnum<string>]` row family carrying each protocol binding this folder owns: content-mode reach, attribute placement and prefix, routing key, filter-pushdown verdict, the `Reader` projecting one dialect message onto the neutral `BrokerDelivery`, the `Subscribe` opener over a `BrokerSource`, and the `LaneBound` every row declares over whichever queue its delivery shape rides; `BrokerSource` `[Union]` the held-client family, one arm per row; `BrokerDelivery` the dialect-neutral carrier — the parent adoption, the body, the framing, and the carried attribute pairs — every decode reads; `SensorReading<T>` the typed body paired with the envelope that carried it, every causal, lag, sampling, and expiry fact PROJECTING off that envelope's own attributes; `CaptureAdmission` the one admission policy row the capture sink reads, its `Absorb` fold fanning ONE delivery onto the ephemeral twin lane and the durable observation lane; `BrokerIngress` the one row carrying this subscription's span source, span name, and carrier trust class; `BrokerChannels` the one `EventExtensionContract<event.Extensions>` bridge, decode, pump, and `Capture` admit sink closing the loop onto `WorkLane.CaptureIngest`.
- Law: the CNCF MQTT binding package is REFUSED at the branch and this folder owns the binding instead — that package pins MQTTnet 4.x against the solution's 5.x, reads a `PayloadSegment` getter the restored v5 message dropped, and reaches structured mode alone, so composing it compiles and then faults `MissingMethodException` on every delivery while forfeiting binary mode entirely. Refusing a package is never refusing the binding: MQTT 5.0 carries BOTH content modes, its attributes ride User Properties UNPREFIXED (the one binding in the matrix that prefixes nothing, because v5 gives properties their own namespace), and the topic is its routing key.
- Law: NATS is branch-owned for the same reason — the CNCF NATS binding targets net6.0/netstandard2.0 against the retired v1 client while this folder holds the current `NATS.Net` line — so its rows lower onto `NatsHeaders` under the specification's `ce-` prefix (NATS ≥ 2.2 carries headers) with the subject as the routing key.
- Law: one decode and one pump serve BOTH rows, because the only per-dialect variance is the CARRIER READ — which member holds the body, which holds the framing, which holds the attribute pairs, and how the parent adopts. Those four are the row's `Reader` column, so the page's own claim that a third protocol is one `BrokerBinding` row is true rather than aspirational. NAMED LOSS: the dialect message type no longer appears in a public signature, so a caller holding a raw `MqttApplicationMessage` composes the row's reader instead of a named `Mqtt<T>` overload. Witness: the two overloads carried byte-identical decode bodies and the pump pair carried byte-identical fault folds, so a fix landed at one and drifted at the other.
- Law: `recordedtime` is the PRODUCER's CloudEvent-creation stamp and ingress preserves it. `SensorReading.Received` is the receiver's interior arrival stamp; `Recorded - Occurred` measures occurrence-to-recording delay and `Received - Recorded` measures delivery delay. Either interval is absent when its source stamp is absent or reverses time, so no fabricated zero or negative wait reaches scoring.
- Law: `expirytime` DROPS a stale reading at admission rather than scoring it — a twin surrogate fed a reading whose delivery window closed reports a present state from a past world, which is worse than reporting nothing — so the expiry gate runs before the fan, not inside it, and the drop counts on the lane-pressure gate rather than passing silent.
- Law: `sampledrate` declares the producer's head-sampling denominator and the twin weights by it, so a stream publishing one reading in ten contributes ten readings' worth of evidence rather than one; an absent row reads as unsampled, the only honest default, because a producer that samples always says so.
- Law: the inbound body bound is the package's ONE inbound-parse policy. `Runtime/channels#ARTIFACT_FRAMES` `WireLimits.Inbound.SizeLimit` is the bound every foreign buffer this package decodes crosses, so a broker body and a gRPC message are refused on one declared ceiling and a second literal here would be a per-dialect ceiling nothing reconciles. NAMED LOSS: `ParseGuard.Read` itself does not compose on this leg — it parses a generated protobuf message and a broker body is a CloudEvents envelope — so the policy VALUE crosses and the parse stays the kernel decode's.
- Law: a lane's bound is ROW data and EVERY row carries one, because every delivery shape queues before this package reads it. The MQTT bridge takes its `LaneBound` from the row rather than a `capacity` parameter, and its full-mode is `Parked` because the ack rides the successful enqueue: a shedding bridge would ack nothing and the broker's redelivery is the whole recovery. NAMED LOSS: a composition can no longer widen one subscription's bridge without a row edit. Witness: a call-site capacity made the backpressure decision unrecoverable from any declaration, so no conservation audit could walk it.
- Law: the NATS row's bound is the CLIENT's pending channel, declared here and lowered onto `NatsSubOpts.ChannelOpts` rather than re-opened as a second queue. `NatsSub<T>` builds a bounded channel over `NatsOpts.SubPendingChannelCapacity`/`SubPendingChannelFullMode` on every subscription, so the prior `Option<LaneBound>.None` did not mean unbounded — it meant `16384`/`DropNewest` chosen silently, with an unreachable `None` arm minting an unbounded channel at the one construction site. Stating the row makes the bound a declaration a conservation audit can walk, and the `Option` collapses because no row inhabits it. NAMED LOSS: a protocol that genuinely queues nowhere can no longer say so without taking the `Option` back and naming its discriminant.
- Law: NATS parks NOWHERE. Parking the pending channel stalls the client's socket read loop, the server severs that connection as a slow consumer, and every subject riding it dies together, so the row sheds and the shedding arm is the only `LaneBound` case carrying a lowering — parked and ranked refuse at `Bounded` rather than degrading to the package default.
- Law: a Core drop is COUNTED because nothing replays it. MQTT's bridge drops unobserved by design — the unacked QoS 1/2 delivery redelivers and the loss class is the broker's — while Core NATS carries no redelivery at all, so the identical silence renders a permanently lost sensor reading indistinguishable from one never published. `INatsConnection.MessageDropped` is the client's own overflow surface and it feeds the same refusal channel the expiry gate uses; the handler discriminates on `NatsMessageDroppedEventArgs.Subscription` identity, since under a wildcard filter the args' `Subject` is the PUBLISHED subject and matches no subscription's own spelling.
- Law: the NATS subscription closes by FLUSHING. `SubscribeCoreAsync` holds the `INatsSub<byte[]>` whose `DrainAsync` sends UNSUB, fences on a PING/PONG round trip bounded by `NatsOpts.DrainPingTimeout`, and only then completes `Msgs`, so readings already bounded and already admitted reach the capture lane; the `SubscribeAsync` enumerable exposes no such verb, and cancelling it abandons exactly the window this row declared. NAMED LOSS: teardown now costs one round trip. Witness: a token-only stop discarded every buffered reading while every drop before it had been carefully accounted.
- Law: subscription QoS is fixed at `AtLeastOnce` by the ack law rather than offered as a parameter. QoS 0 carries no redelivery, which makes `AutoAcknowledge = false` and `ProcessingFailed` pure ceremony, and QoS 2 pays a second round trip for an exactly-once guarantee the expiry gate and the content-keyed chunk already make unnecessary. NAMED LOSS: a deployment wanting fire-and-forget telemetry now states it as a row, not a call.
- Entry: `BrokerChannels.Decode<T>(BrokerIngress ingress, BrokerBinding binding, BrokerDelivery delivery, ClockPolicy clocks)` is the ONE message-to-reading adapter — it captures the receiver arrival once, opens the consumer bracket through the delivery's own adoption, takes the STRUCTURED leg through `EventEnvelope.Decode` when the framing names an admitted event format and the BINARY leg through `BrokerCodec.Raise` over the row's prefixed carrier otherwise, preserves the producer's `recordedtime`, and projects the typed body. `BrokerChannels.Pump<T>(BrokerIngress ingress, BrokerBinding binding, BrokerSource source, string filter, ClockPolicy clocks, CancellationToken ct)` is the ONE subscription pump, yielding `IAsyncEnumerable<Fin<SensorReading<T>>>` — a subscribe refusal, a severed session, or an enumeration failure yields one CLASSIFIED terminal fault and ends the stream, and cancellation rethrows. `Error.New(IAsyncEnumerable<Fin<SensorReading<TwinSignal>>> deliveries.Message, IAsyncEnumerable<Fin<SensorReading<TwinSignal>>> deliveries)` is the sink closing the loop — each delivery folds through `CaptureAdmission.Absorb`, and a refusal on either leg parks on the injected arrow rather than ending the subscription.
- Law: a terminal fault carries the transience its cause has. A broker that refuses a subscription with `NotAuthorized` or `TopicFilterInvalid` is deterministic and lands on an arm inheriting the kernel `Terminal` default, while a quota exceedance, a severed session, a connection failure, and a timeout land on `ComputeFault.EndpointUnreachable`, which PUBLISHES `Retriability.Transient` at its owner. NAMED LOSS: the one-string catch-all was shorter. Witness: it folded a disposed client and an invalid topic filter onto the same transient arm, so the re-drive policy re-attempted a refusal that answers identically forever, and it discarded the SUBACK result entirely, so a `NotAuthorized` grant read as a healthy subscription that simply never delivered.
- Auto: content mode resolves from the message itself, never a subscription knob. Structured and binary legs both obtain declarations and whole-message admission from one `EventExtensionContract<event.Extensions>`, then derive lag, expiry, sampling, and creation trace from that message. NATS control frames resolve inside the binding reader and never reach decode.
- Law: `Capture` admits each typed reading onto `WorkLane.CaptureIngest` through the one `AdmittedIntent` gate; the NATS `queueGroup` load-balances one subject across N capture subscribers.
- Growth: a new typed broker body reuses `Decode<T>`; a generated extension changes `event.proto` and the descriptor-total bridge consumes it without a local field projection. A new admission stance is one `CaptureAdmission` value, a third protocol one `BrokerBinding` row, and request/reply rides the existing connection.
- Boundary: MQTT reads `ValueBuffer` through the package helper and acknowledges only a successful bridge enqueue. `TraceContext.Continue` owns current-hop adoption; the admitted generated message owns creation-time trace. NATS holds one long-lived client and opens no queue of its own — the row's bound crosses into the client's pending channel and the refusal log is the only channel this arm constructs. CloudEvents framing stays kernel-owned, while extension names and value spaces derive from the generated descriptor and no attribute literal or local roster survives. Foreign envelopes pass the generic `EventEnvelope` gate without being forced through the Rasm type/source/id grammar.

Both rows reach binary and structured content mode and both resolve a subscription filter at the broker, so those two coordinates carry no per-row signal and stay out of the table; placement, routing, and what each row does when its queue overruns are where the rows genuinely diverge.

| [INDEX] | [BINDING] | [PLACEMENT]                 | [ROUTES_ON] | [BOUND]                          | [OVERRUN]                      |
| :-----: | :-------- | :-------------------------- | :---------- | :------------------------------- | :----------------------------- |
|  [01]   | `mqtt`    | User Properties, UNPREFIXED | topic       | parked bridge this page opens    | unacked; the broker redelivers |
|  [02]   | `nats`    | headers, `ce-` prefixed     | subject     | shedding channel the client owns | counted; nothing replays it    |

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record BrokerSource {
    private BrokerSource() { }

    public sealed record Mqtt(IMqttClient Client) : BrokerSource;
    public sealed record Nats(INatsClient Client, Option<string> QueueGroup) : BrokerSource;
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record BrokerDelivery(
    Func<BrokerIngress, IDisposable> Adopt,
    ReadOnlyMemory<byte> Body,
    Option<ContentType> Framing,
    Seq<(string Name, string Value)> Carried);

public sealed record SensorReading<T>(
    CloudEvent Envelope,
    global::Rasm.Contracts.Event.Extensions Extensions,
    T Data,
    Instant Received) {
    public TraceCarrier Trace => TraceCarrier.Admit(
        Extensions.HasTraceparent ? Extensions.Traceparent : null,
        Extensions.HasTracestate ? Extensions.Tracestate : null,
        Extensions.HasBaggage ? Extensions.Baggage : null);

    public Option<Instant> Occurred => Optional(Envelope.Time).Map(static at => Instant.FromDateTimeOffset(at));

    public Option<Instant> Recorded => Optional(Extensions.Recordedtime).Map(static value => Instant.FromDateTimeOffset(value.ToDateTimeOffset()));

    public Option<Duration> RecordingLag =>
        from recorded in Recorded
        from occurred in Occurred
        where recorded >= occurred
        select recorded - occurred;

    public Option<Duration> DeliveryLag =>
        from recorded in Recorded
        where Received >= recorded
        select Received - recorded;

    public int Sampled => Extensions.HasSampledrate ? checked((int)Extensions.Sampledrate) : 1;

    public bool Expired(Instant now) =>
        Optional(Extensions.Expirytime).Map(static value => Instant.FromDateTimeOffset(value.ToDateTimeOffset())).Match(
            Some: expiry => now > expiry,
            None: static () => false);
}

public sealed record BrokerIngress(ActivitySource Source, TenantAdoption Adoption, string Span);

// --- [TABLES] --------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class BrokerBinding {
    public static readonly BrokerBinding Mqtt = new("mqtt",
        prefix: "", routes: "topic", pushdown: true, system: "mqtt",
        bridge: new LaneBound.Parked(1024),
        subscribe: MqttBinding.Subscribe);

    public static readonly BrokerBinding Nats = new("nats",
        prefix: "ce-", routes: "subject", pushdown: true, system: "nats",
        bridge: new LaneBound.Shedding(16384, BoundedChannelFullMode.DropNewest),
        subscribe: NatsBinding.Subscribe);

    public string Prefix { get; }

    public string System { get; }

    public string Routes { get; }

    public bool Pushdown { get; }

    public LaneBound Bridge { get; }

    public Func<BrokerSource, string, LaneBound, CancellationToken, IAsyncEnumerable<Fin<BrokerDelivery>>> Subscribe { get; }

    public string Wire(string attribute) => Prefix + attribute;

    public Option<string> Attribute(string carried) =>
        carried.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase) ? Some(carried[Prefix.Length..]) : None;
}

// --- [BOUNDARIES] ----------------------------------------------------------------------
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

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class BrokerChannels {
    private static readonly EventExtensionContract<global::Rasm.Contracts.Event.Extensions> ExtensionContract = new(
        global::Rasm.Contracts.Event.Extensions.Parser,
        global::Rasm.Contracts.Event.Extensions.Descriptor,
        new global::Celly.Protovalidate.Validator([
            global::Rasm.Contracts.Event.EventReflection.Descriptor,
        ]));

    public static class BrokerCodec {
        public static Fin<CloudEvent> Structured(ReadOnlyMemory<byte> body, ContentType framing) =>
            from declared in ExtensionContract.Declarations()
            from rows in EventEnvelope.Decode(new EventFrame(Body: body, Framing: framing), declared)
            from single in rows is [CloudEvent envelope]
                ? Fin.Succ(envelope)
                : Fin.Fail<CloudEvent>(new ComputeFault.WireDecodeRejected($"<broker-batch-on-stream:{rows.Count}>"))
            select single;

        public static Fin<CloudEvent> Raise(
            BrokerBinding binding, Seq<(string Name, string Value)> carried, ReadOnlyMemory<byte> body,
            Option<ContentType> dataType) =>
            from declared in ExtensionContract.Declarations()
            from envelope in EventEnvelope.Raise(
                attributes: carried.Choose(row => binding.Attribute(row.Name).Map(name => (Name: name, Value: row.Value))),
                declared: declared,
                data: body, dataType: dataType)
            select envelope;
    }

    public static Fin<SensorReading<T>> Decode<T>(
        BrokerIngress ingress, BrokerBinding binding, BrokerDelivery delivery, ClockPolicy clocks) {
        Instant received = clocks.Now;
        using IDisposable adopted = delivery.Adopt(ingress);
        return Bounded(delivery.Body)
            .Bind(body => delivery.Framing.Filter(static type => EventFormat.Of(type).IsSome).Match(
                Some: type => BrokerCodec.Structured(body, type),
                None: () => BrokerCodec.Raise(binding, delivery.Carried, body, delivery.Framing)))
            .Bind(envelope => ExtensionContract.Admit(envelope)
                .Map(extensions => (Envelope: envelope, Extensions: extensions, Received: received)))
            .Bind(Project<T>);
    }

    public static async IAsyncEnumerable<Fin<SensorReading<T>>> Pump<T>(
        BrokerIngress ingress,
        BrokerBinding binding,
        BrokerSource source,
        string filter,
        ClockPolicy clocks,
        [EnumeratorCancellation] CancellationToken ct = default) {
        await foreach (Fin<BrokerDelivery> delivery in binding.Subscribe(source, filter, binding.Bridge, ct).WithCancellation(ct).ConfigureAwait(false)) {
            yield return delivery.Bind(admitted => Decode<T>(ingress, binding, admitted, clocks));
        }
    }

    private static Fin<ReadOnlyMemory<byte>> Bounded(ReadOnlyMemory<byte> body) =>
        body.Length <= WireLimits.Inbound.SizeLimit
            ? Fin.Succ(body)
            : Fin.Fail<ReadOnlyMemory<byte>>(new ComputeFault.PayloadOverBounds(
                $"<broker-body-over-bound:{body.Length}:{WireLimits.Inbound.SizeLimit}>"));

    private static Fin<SensorReading<T>> Project<T>(
        (CloudEvent Envelope, global::Rasm.Contracts.Event.Extensions Extensions, Instant Received) admitted) =>
        admitted.Envelope.Data is T data
            ? Fin.Succ(new SensorReading<T>(admitted.Envelope, admitted.Extensions, data, admitted.Received))
            : Fin.Fail<SensorReading<T>>(new ComputeFault.WireDecodeRejected($"<broker-reading-data:{admitted.Envelope.Id}>"));

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
internal static class MqttBinding {
    public static async IAsyncEnumerable<Fin<BrokerDelivery>> Subscribe(
        BrokerSource source, string topicFilter, LaneBound bound,
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

    private static async Task<Option<Error>> Granted(IMqttClient client, string topicFilter, CancellationToken ct) =>
        (await HostEdge.Captured(
            async _ => Fin.Succ(await client.SubscribeAsync(
                new MqttClientSubscribeOptionsBuilder()
                    .WithTopicFilter(topicFilter, MqttQualityOfServiceLevel.AtLeastOnce)
                    .Build(),
                ct).ConfigureAwait(false))).ConfigureAwait(false))
        .Match(
            Succ: result => toSeq(result.Items)
                .Find(static item => item.ResultCode >= MqttClientSubscribeResultCode.UnspecifiedError)
                .Map(item => Refusal(topicFilter, item.ResultCode)),
            Fail: Some);

    private static Error Refusal(string topicFilter, MqttClientSubscribeResultCode code) =>
        code is MqttClientSubscribeResultCode.QuotaExceeded
            ? new ComputeFault.EndpointUnreachable($"<mqtt-subscribe:{topicFilter}:{code}>")
            : new ComputeFault.Violation(ComputeArea.Runtime, new ComputeViolation.Contract(ComputeContract.Valid, new ContractEvidence.Status((int)code)));

    private static Fin<Channel<MqttApplicationMessage>> Bridged(LaneBound bound) =>
        bound.Open(new LaneChannel<MqttApplicationMessage>(
            Readers: 1, InlineContinuations: false, Dropped: static (_, _) => { }, Rank: None));

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
internal static class NatsBinding {
    public static async IAsyncEnumerable<Fin<BrokerDelivery>> Subscribe(
        BrokerSource source, string subject, LaneBound bound,
        [EnumeratorCancellation] CancellationToken ct = default) {
        if (source is not BrokerSource.Nats { Client: { } client, QueueGroup: { } queueGroup }) {
            yield return Fin.Fail<BrokerDelivery>(new ComputeFault.Violation(ComputeArea.Runtime, new ComputeViolation.Contract(ComputeContract.Compatible, new ContractEvidence.Type(source.GetType()))));
            yield break;
        }

        if (Bounded(bound) is not { Case: NatsSubOpts opts }) {
            yield return Fin.Fail<BrokerDelivery>(new ComputeFault.LaneUnprofiled(
                $"<nats-bound-not-shedding:{bound.GetType().Name}:{subject}>"));
            yield break;
        }

        INatsConnection connection = client.Connection;
        INatsSub<byte[]> subscription = await connection
            .SubscribeCoreAsync<byte[]>(subject, queueGroup.IfNoneUnsafe(() => null), opts: opts, cancellationToken: ct)
            .ConfigureAwait(false);

        if (bound.Open(new LaneChannel<Error>(
                Readers: 1, InlineContinuations: false, Dropped: static (_, _) => { }, Rank: None))
            .Case is not Channel<Error> shed) {
            yield return Fin.Fail<BrokerDelivery>(new ComputeFault.LaneUnprofiled($"<nats-shed-log-unranked:{subject}>"));
            yield break;
        }

        AsyncEventHandler<NatsMessageDroppedEventArgs> dropped = (_, args) => {
            if (ReferenceEquals(args.Subscription, subscription)) {
                shed.Writer.TryWrite(new ComputeFault.PayloadOverBounds(
                    $"<nats-pending-overrun:{args.Subject}:{args.Pending}>"));
            }
            return ValueTask.CompletedTask;
        };
        connection.MessageDropped += dropped;

        try {
            await foreach (Fin<BrokerDelivery> yielded in Drain(subscription, subject, shed, ct).ConfigureAwait(false)) {
                yield return yielded;
            }
        } finally {
            connection.MessageDropped -= dropped;
            shed.Writer.TryComplete();
            await subscription.DrainAsync(CancellationToken.None).ConfigureAwait(false);
            await subscription.DisposeAsync().ConfigureAwait(false);
        }
    }

    private static Option<NatsSubOpts> Bounded(LaneBound bound) => bound.Switch(
        shedding: static row => Some(new NatsSubOpts {
            ChannelOpts = new NatsSubChannelOpts { Capacity = row.Capacity, FullMode = row.Mode },
        }),
        parked: static _ => Option<NatsSubOpts>.None,
        ranked: static _ => Option<NatsSubOpts>.None);

    private static async IAsyncEnumerable<Fin<BrokerDelivery>> Drain(
        INatsSub<byte[]> subscription, string subject, Channel<Error> shed,
        [EnumeratorCancellation] CancellationToken ct = default) {
        while (true) {
            Fin<bool> advanced = await HostEdge.Captured(
                async _ => Fin.Succ(await subscription.Msgs.WaitToReadAsync(ct).ConfigureAwait(false))).ConfigureAwait(false);

            while (shed.Reader.TryRead(out Error? refusal)) {
                yield return Fin.Fail<BrokerDelivery>(refusal);
            }

            if (advanced.Case is Error refused) { yield return Fin.Fail<BrokerDelivery>(refused); yield break; }
            if (advanced.Case is false) { yield break; }
            while (subscription.Msgs.TryRead(out NatsMsg<byte[]> message)) {
                if (message.IsEmpty || message.HasNoResponders) { continue; }
                yield return Fin.Succ(Read(message, subject));
            }
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
- Law: a status is a DISCRIMINANT, not a slug. A 429 carrying a delta `Retry-After` lands `ComputeFault.EndpointThrottled`, whose `Retriability.Throttled(RetryAfter)` carries the server-declared window onto the re-drive policy; a window-less rate limit, a timeout, and a server outage recover on a later attempt and land `ComputeFault.EndpointUnreachable` (`Transient`); a not-found, an unauthorized, and a malformed request answer identically forever and land arms inheriting the kernel `Terminal` default. Witness: every non-2xx previously folded onto one transient arm, so a 404 for an unknown class re-drove until the budget was spent and a throttled peer was re-driven on the policy's own curve rather than the window it named.
- Law: a consumer-typed generic mounts an OPEN resolver. `TypeInfoResolver = Context.Default` resolves only the roots that context registered, and `Fetch<TResponse>` is by construction handed shapes this package never sees, so the options instance declares the reflection-backed resolver EXPLICITLY rather than inheriting it as the silent fallback a source-generated contract claims it deleted.
- Output: the REST hop's route, status, byte sizes, deadline outcome, and wall settle as one `Runtime/channels#CALL_POLICY` `RemoteReply` returned by `AwaitedHttp`.
- Packages: LanguageExt.Core, NodaTime (`Duration` on the throttle arm), Rasm.AppHost (project — `DeadlineClass`), BCL inbox (`System.Net.Http`, `System.Text.Json`, `System.Text.Json.Serialization.Metadata`)
- Growth: a new dictionary query is one `Fetch<TResponse>` call with its own response shape; a new status classification is one arm on the status fold; zero new surface.
- Boundary: a transport miss returns the typed fault the app-root `BsddPort` adapter degrades on, and the app composition root that references both packages closes `Fetch<BsddClassResponse>` and adapts it into the Bim `BsddPort` so neither package depends on the other — a Bim-minted bSDD transport, a Compute-side bSDD response record or local fallback, and a direct cross-package reference in either direction are the rejected forms. A 200 carrying an empty body is a CONTENT refusal and fails as one: the prior `?? throw` was re-caught two frames up and re-labelled unreachable, so an endpoint answering correctly with nothing read as an endpoint that could not be reached.

```csharp
// --- [BOUNDARIES] ----------------------------------------------------------------------
public sealed class BsddTransport(HttpClient client, CallSpine spine) {
    private static readonly JsonSerializerOptions BsddWire = new(JsonSerializerDefaults.Web) {
        PropertyNameCaseInsensitive = true,
        TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
    };

    public IO<Fin<TResponse>> Fetch<TResponse>(string classUri, CancellationToken token) =>
        spine.AwaitedHttp(classUri, token, async (uri, scope) => {
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

    private static Error Refusal(string uri, HttpStatusCode status, Option<TimeSpan> retryAfter) =>
        status is HttpStatusCode.TooManyRequests && retryAfter.Case is TimeSpan window
            ? new ComputeFault.EndpointThrottled($"<bsdd:429:{uri}>", Duration.FromTimeSpan(window))
            : status is HttpStatusCode.TooManyRequests or HttpStatusCode.RequestTimeout or >= HttpStatusCode.InternalServerError
                ? new ComputeFault.EndpointUnreachable($"<bsdd:{(int)status}:{uri}>")
                : new ComputeFault.Violation(ComputeArea.Runtime, new ComputeViolation.Contract(ComputeContract.Valid, new ContractEvidence.Status((int)status)));
}

public abstract partial record ComputeFault {
    [FaultCase(27)] public sealed partial record WireDecodeRejected(string Detail) : ComputeFault(Detail);

    [FaultCase(28)] public sealed partial record EndpointThrottled(string Detail, Duration RetryAfter) : ComputeFault(Detail) {
        public override Retriability Retriability => Retriability.Throttled(RetryAfter);
    }
}
```

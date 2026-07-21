# [RASM_PERSISTENCE_API_MQTT]

`MQTTnet` supplies the MQTT v5 client backing `EgressSink.Mqtt` — the outbound publish-and-ack leg of the CDC egress pump. `MqttClientFactory.CreateMqttClient()` mints a per-instance `IMqttClient`; `MqttApplicationMessageBuilder` assembles the CloudEvents-projected payload with its v5 `UserProperties` tracing carrier and a QoS-1 `MqttQualityOfServiceLevel.AtLeastOnce` delivery flag; awaited `PublishAsync` returns `MqttClientPublishResult` — the PUBACK evidence whose `ReasonCode`/`IsSuccess` folds to `DeliveryAck` at the sink boundary. This overlay scopes the outbound egress surface; the AppHost `api-mqtt` catalogue owns the inbound live-external industrial-wire surface (connect, subscribe, the receive pump, will, topic filters) over the same package. It backs one `EgressSink` case over the shared `Egress.Envelope` projection, distinct from the Kafka (`api-kafka`), NATS JetStream (`api-nats`), RabbitMQ (`api-rabbitmq`), Pulsar (`api-dotpulsar`), and AMQP 1.0 (`api-cloudevents`) egress wire protocols, and pairs with `CloudNative.CloudEvents.Mqtt` as the envelope-to-application-message projection peer.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `MQTTnet`
- package: `MQTTnet`
- license: MIT (.NET Foundation and Contributors); full free use, OSS admit
- assembly: `MQTTnet`
- namespace: `MQTTnet` (client, factory, message, options, result values), `MQTTnet.Protocol` (QoS and retain enums), `MQTTnet.Packets` (`MqttUserProperty`), `MQTTnet.Formatter` (`MqttProtocolVersion`)
- target: multi-target (`net10.0`, `net8.0`); the consumer binds `lib/net10.0` (the bound asset, highest-precedence)
- native: pure-managed (no `runtimes/<rid>/native` payload); the MQTT control-packet framing rides the client's own socket/WebSocket channel
- transitive: dependency-free — both nuspec dependency groups (`net10.0`, `net8.0`) are empty, so the install gate resolves with zero added closure
- xml docs: present (`MQTTnet.xml` ships per TFM — member intent is doc-comment-sourced)
- rail: egress-sink

`IMqttClient` is `IDisposable` and its type doc declares it safe for concurrent publish/subscribe/ping — outgoing packets serialize internally and acknowledgements route back to the awaiting caller by packet identifier; `ConnectAsync`/`DisconnectAsync` alone must not race each other or a publish. The async surface is `Task`-based; publish payloads carry as `ReadOnlySequence<byte>` on `MqttApplicationMessage.Payload`.

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: client, factory, and publish result
- rail: egress-sink

`MqttClientFactory` is the single construction root: `CreateMqttClient()` mints a fresh per-instance `IMqttClient` — a per-sink-leg handle the composition root binds through `SinkBinding.Leg`, never a process-global singleton, so one bound client owns one egress connection and its lifecycle disposes with the sink.

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY]   | [RAIL]                                                        |
| :-----: | :------------------------ | :-------------- | :------------------------------------------------------------ |
|  [01]   | `MqttClientFactory`       | factory         | mints the per-instance `IMqttClient` and the fluent builders  |
|  [02]   | `IMqttClient`             | client handle   | `IDisposable`; per-instance connection; `PublishAsync` egress |
|  [03]   | `MqttClientPublishResult` | PUBACK evidence | `ReasonCode`/`IsSuccess`/`PacketIdentifier`/`UserProperties`  |
|  [04]   | `MqttClientOptions`       | options value   | the built connection configuration `ConnectAsync` consumes    |

[PUBLIC_TYPE_SCOPE]: message, builders, and the v5 tracing carrier
- rail: egress-sink

`MqttApplicationMessageBuilder` assembles the envelope payload; `MqttApplicationMessage.UserProperties` (`List<MqttUserProperty>`) carries the W3C `traceparent`/`tracestate` rows so every egress publish joins the drain trace, mirroring the NATS `Nats-Msg-Id` and Kafka `ce_*` header carriers.

| [INDEX] | [SYMBOL]                        | [TYPE_FAMILY] | [RAIL]                                                      |
| :-----: | :------------------------------ | :------------ | :---------------------------------------------------------- |
|  [01]   | `MqttApplicationMessage`        | message value | `Topic`/`Payload`/`QualityOfServiceLevel`/`UserProperties`  |
|  [02]   | `MqttApplicationMessageBuilder` | message       | fluent payload assembly; `Build()` materializes the message |
|  [03]   | `MqttClientOptionsBuilder`      | options       | connection assembly; `WithProtocolVersion` selects v5       |
|  [04]   | `MqttUserProperty`              | v5 property   | `Name`/`ValueBuffer`; the tracing key-value carrier pair    |

[PUBLIC_TYPE_SCOPE]: protocol enums
- rail: egress-sink

| [INDEX] | [SYMBOL]                             | [TYPE_FAMILY] | [RAIL]                                           |
| :-----: | :----------------------------------- | :------------ | :----------------------------------------------- |
|  [01]   | `Protocol.MqttQualityOfServiceLevel` | QoS enum      | `AtMostOnce`/`AtLeastOnce` (QoS-1)/`ExactlyOnce` |
|  [02]   | `MqttClientPublishReasonCode`        | PUBACK reason | `Success`/`NoMatchingSubscribers` + 128+ errors  |
|  [03]   | `Formatter.MqttProtocolVersion`      | version enum  | `V500` the egress selector; `V311`/`V310` legacy |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the `EgressSink.Mqtt` Deliver leg
- rail: egress-sink

| [INDEX] | [SURFACE]                                                              | [ENTRY_FAMILY] | [RAIL]                                       |
| :-----: | :--------------------------------------------------------------------- | :------------- | :------------------------------------------- |
|  [01]   | `MqttClientFactory.CreateMqttClient()`                                 | client mint    | the per-instance `IMqttClient` handle        |
|  [02]   | `IMqttClient.ConnectAsync(options, ct)`                                | session op     | `MqttClientConnectResult`; connection open   |
|  [03]   | `IMqttClient.PublishAsync(message, ct)`                                | publish op     | `Task<MqttClientPublishResult>` PUBACK await |
|  [04]   | `MqttApplicationMessageBuilder.WithTopic(topic)`                       | addressing     | the sink topic                               |
|  [05]   | `MqttApplicationMessageBuilder.WithPayloadSegment`                     | payload        | the redacted CloudEvents envelope bytes      |
|  [06]   | `MqttApplicationMessageBuilder.WithQualityOfServiceLevel(AtLeastOnce)` | delivery       | QoS-1 at-least-once                          |
|  [07]   | `MqttApplicationMessageBuilder.WithUserProperty`                       | v5 metadata    | the `traceparent`/`tracestate` carrier rows  |
|  [08]   | `MqttClientOptionsBuilder.WithProtocolVersion(V500)`                   | session        | v5 selection; the carrier and reason codes   |

## [04]-[IMPLEMENTATION_LAW]

[DELIVER_FOLD]:
- the leg awaits `PublishAsync` and folds `MqttClientPublishResult` to `DeliveryAck` at the sink boundary — a raw result never crosses into the pump.
- `IsSuccess` (`ReasonCode == Success` or `NoMatchingSubscribers`) → `Persisted`; a QoS-1 PUBACK confirms broker receipt, and `NoMatchingSubscribers` is a delivered-but-unrouted success, never a fault.
- a transient transport ambiguity (client disconnect, timeout) → `Indeterminate`; the held cursor re-drives and receiver-side dedup on the CloudEvents `id` absorbs the replay.
- a definitive reason-code error (`128`+ — `NotAuthorized`, `TopicNameInvalid`, `QuotaExceeded`, `PayloadFormatInvalid`) → `Refused` with the reason string, and the drain dead-letters the entry.

[TRACING_CARRIER]:
- `WithUserProperty(string, string)` is `[Obsolete]`; the leg builds the trace rows through the `ReadOnlyMemory<byte>`/`ArraySegment<byte>` overload, and `MqttUserProperty.ValueBuffer` reads them back without the obsolete `Value` allocation.
- the v5 `UserProperties` carrier requires `MqttProtocolVersion.V500`; under `V311` the properties and reason codes are silently dropped, so the egress options fix v5.

[ISOLATION]:
- per-instance: `CreateMqttClient()` mints a distinct `IMqttClient` per call — the sink leg owns one client instance, disposed with the sink; no host-wide state and no single-owner admission constraint.
- the composition root fills `SinkBinding.Leg` from the bound client, so the `EgressSink.Mqtt` case never instantiates `MqttClient` directly and provider types never enter the case.

[RAIL_LAW]:
- Package: `MQTTnet`
- Owns: the MQTT v5 outbound publish transport backing `EgressSink.Mqtt` — message assembly, the UserProperties tracing carrier, and QoS-1 PUBACK reason-code results
- Accept: factory-minted clients, builder-composed v5 messages, `CancellationToken`-scoped publish, result-fold-to-`DeliveryAck` at the boundary
- Reject: direct `MqttClient` instantiation, the obsolete string `WithUserProperty` overload, `V311` under the trace carrier, exception-driven publish flow, a raw `MqttClientPublishResult` crossing into the pump

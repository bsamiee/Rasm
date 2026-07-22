# [RASM_PERSISTENCE_API_MQTT]

`MQTTnet` owns the MQTT v5 outbound publish transport backing the `EgressSink.Mqtt` delivery row: `MqttClientFactory` mints one per-sink `IMqttClient`, the CloudEvents-projected `MqttApplicationMessage` publishes at `MqttQualityOfServiceLevel.AtLeastOnce`, and the awaited PUBACK `MqttClientPublishResult` folds to `DeliveryAck` at the sink boundary. `MqttProtocolVersion.V500` opens the `UserProperties` carrier the W3C `traceparent`/`tracestate` pair rides, so every egress publish joins the drain trace.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `MQTTnet`
- package: `MQTTnet` (MIT, .NET Foundation and Contributors)
- assembly: `MQTTnet`
- namespace: `MQTTnet`, `MQTTnet.Protocol`, `MQTTnet.Packets`, `MQTTnet.Formatter`
- asset: pure-managed runtime library; control-packet framing rides the client's own socket or WebSocket channel
- rail: egress-sink

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the v5 publish transport model

| [INDEX] | [SYMBOL]                             | [TYPE_FAMILY] | [CAPABILITY]                                   |
| :-----: | :----------------------------------- | :------------ | :--------------------------------------------- |
|  [01]   | `MqttClientFactory`                  | class         | per-instance client and builder mint           |
|  [02]   | `IMqttClient`                        | interface     | `IDisposable` session; publish, ping, teardown |
|  [03]   | `MqttClientOptionsBuilder`           | class         | session, will, and v5 flow-control assembly    |
|  [04]   | `MqttClientExtensions`               | class         | sequence publish, reconnect, swallowing probes |
|  [05]   | `MqttApplicationMessage`             | class         | topic, payload sequence, QoS, v5 properties    |
|  [06]   | `Packets.MqttUserProperty`           | class         | v5 `Name`/`ValueBuffer` metadata pair          |
|  [07]   | `MqttClientConnectResult`            | class         | CONNACK; session-present and broker v5 limits  |
|  [08]   | `MqttClientPublishResult`            | class         | PUBACK reason, reason string, packet id        |
|  [09]   | `Protocol.MqttQualityOfServiceLevel` | enum          | `AtMostOnce`/`AtLeastOnce`/`ExactlyOnce`       |
|  [10]   | `MqttClientPublishReasonCode`        | enum          | `Success`, `NoMatchingSubscribers`, `128`+     |
|  [11]   | `Formatter.MqttProtocolVersion`      | enum          | `V500` selects the v5 property plane           |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the `EgressSink.Mqtt` deliver leg — client mint, session assembly, publish, teardown

| [INDEX] | [SURFACE]                                                                         | [SHAPE]  | [CAPABILITY]                             |
| :-----: | :-------------------------------------------------------------------------------- | :------- | :--------------------------------------- |
|  [01]   | `MqttClientFactory.CreateMqttClient()`                                            | factory  | one client per sink leg                  |
|  [02]   | `MqttClientOptionsBuilder.WithProtocolVersion(MqttProtocolVersion)`               | instance | v5 property and reason-code plane        |
|  [03]   | `MqttClientOptionsBuilder.WithCleanStart(bool)`                                   | instance | resume a broker-held session             |
|  [04]   | `MqttClientOptionsBuilder.WithSessionExpiryInterval(uint)`                        | instance | broker hold on in-flight QoS-1 state     |
|  [05]   | `MqttClientOptionsBuilder.WithRequestProblemInformation(bool)`                    | instance | broker returns `ReasonString` on refusal |
|  [06]   | `MqttClientOptionsBuilder.Build()`                                                | instance | `MqttClientOptions`                      |
|  [07]   | `IMqttClient.ConnectAsync(MqttClientOptions, CancellationToken)`                  | instance | `MqttClientConnectResult` CONNACK        |
|  [08]   | `IMqttClient.PublishAsync(MqttApplicationMessage, CancellationToken)`             | instance | `MqttClientPublishResult` PUBACK         |
|  [09]   | `PublishSequenceAsync(string, ReadOnlySequence<byte>, MqttQualityOfServiceLevel)` | static   | zero-copy publish, no message value      |
|  [10]   | `TryPingAsync(CancellationToken) -> bool`                                         | static   | pre-batch liveness probe                 |
|  [11]   | `ReconnectAsync(CancellationToken)`                                               | static   | re-drive on the held options             |
|  [12]   | `DisconnectAsync(MqttClientDisconnectOptionsReason, string)`                      | static   | reason-coded sink teardown               |
|  [13]   | `MqttUserProperty(string, ReadOnlyMemory<byte>)`                                  | ctor     | one trace row over the buffer carrier    |
|  [14]   | `MqttUserPropertyExtensions.ReadValueAsString()`                                  | static   | read a returned property value back      |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `MqttClientFactory.CreateMqttClient()` mints a distinct `IMqttClient` per call, so one sink leg owns one connection and disposes it with the sink.
- `IMqttClient` serializes outgoing packets internally and routes each acknowledgement to its awaiting caller by packet identifier, so concurrent publishes are safe while `ConnectAsync` and `DisconnectAsync` hold the connection alone.
- `MqttApplicationMessage.Payload` is a `ReadOnlySequence<byte>`, so a large buffered envelope publishes over its own segments with no boundary re-buffer.
- `WithCleanStart(false)` under a non-zero `WithSessionExpiryInterval` holds in-flight QoS-1 state at the broker across a reconnect, and `MqttClientConnectResult.IsSessionPresent` reports whether it did.

[STACKING]:
- `api-cloudevents-mqtt`(`.api/api-cloudevents-mqtt.md`): `MqttExtensions.ToMqttApplicationMessage(ContentMode.Structured, formatter, topic)` returns the exact `MqttApplicationMessage` `PublishAsync` consumes; the leg appends only the trace rows to `UserProperties` before the publish.
- `api-cloudevents`(`.api/api-cloudevents.md`): `Egress.Envelope` mints the `CloudEvent` `id` MQTT carries as its sole replay key, so receiver-side dedup on that id absorbs every held-cursor re-drive.
- Within-lib: `EgressSink.Mqtt` binds the factory-minted client into `SinkBinding.Leg`, whose fold maps `IsSuccess` to `Persisted`, a transport ambiguity to `Indeterminate`, and a `128`+ `ReasonCode` carrying its `ReasonString` to a dead-lettering `Refused`.

[LOCAL_ADMISSION]:
- `SinkBinding.Leg` takes the bound client from the composition root, so the `EgressSink.Mqtt` case constructs no client and no provider type enters the case.

[RAIL_LAW]:
- Package: `MQTTnet`
- Owns: the MQTT v5 outbound publish transport backing `EgressSink.Mqtt` — session assembly, the `UserProperties` tracing carrier, and QoS-1 PUBACK reason-code results
- Accept: factory-minted clients, builder-composed v5 sessions, `CancellationToken`-scoped publish, result folded to `DeliveryAck` at the boundary
- Reject: direct `MqttClient` instantiation, a hand-rolled MQTT packet framer, exception-driven publish flow, a raw `MqttClientPublishResult` crossing into the pump

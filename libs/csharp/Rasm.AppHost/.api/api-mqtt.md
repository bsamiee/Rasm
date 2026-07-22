# [RASM_APPHOST_API_MQTT]

`MQTTnet` owns managed MQTT `MqttProtocolVersion.V311` and v5 client transport: `MqttClientFactory` builds one `IMqttClient` that connects, publishes, and subscribes through fluent builder-composed options and returns reason-code-typed results. AppHost binds it behind the one `TransportRow` adapter on the outbound live-wire seam, decoding inbound `ReadOnlySequence<byte>` payloads to `ExternalValue` and mapping every MQTT reason code to a typed receipt at the boundary.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `MQTTnet`
- package: `MQTTnet` (`MIT`, The Contributors)
- assembly: `MQTTnet`
- namespace: `MQTTnet`, `MQTTnet.Protocol`, `MQTTnet.Formatter`
- asset: runtime library
- rail: outbound

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: client, factory, and extension surfaces

| [INDEX] | [SYMBOL]                         | [TYPE_FAMILY] | [CAPABILITY]                     |
| :-----: | :------------------------------- | :------------ | :------------------------------- |
|  [01]   | `IMqttClient`                    | interface     | connect, publish, subscribe      |
|  [02]   | `MqttClientFactory`              | class         | client and builder construction  |
|  [03]   | `MqttClientExtensions`           | class         | string/binary publish, reconnect |
|  [04]   | `IMqttClientChannelOptions`      | interface     | TCP/WebSocket channel selector   |
|  [05]   | `IMqttClientCredentialsProvider` | interface     | username and password provider   |

[PUBLIC_TYPE_SCOPE]: fluent builders

| [INDEX] | [SYMBOL]                              | [TYPE_FAMILY] | [CAPABILITY]                       |
| :-----: | :------------------------------------ | :------------ | :--------------------------------- |
|  [01]   | `MqttClientOptionsBuilder`            | class         | connection options assembly        |
|  [02]   | `MqttClientTlsOptionsBuilder`         | class         | TLS channel options assembly       |
|  [03]   | `MqttClientWebSocketOptionsBuilder`   | class         | WebSocket channel options assembly |
|  [04]   | `MqttApplicationMessageBuilder`       | class         | publish payload assembly           |
|  [05]   | `MqttClientSubscribeOptionsBuilder`   | class         | subscribe filter set assembly      |
|  [06]   | `MqttClientUnsubscribeOptionsBuilder` | class         | unsubscribe topic set assembly     |
|  [07]   | `MqttClientDisconnectOptionsBuilder`  | class         | graceful disconnect assembly       |
|  [08]   | `MqttTopicFilterBuilder`              | class         | single topic filter assembly       |

[PUBLIC_TYPE_SCOPE]: options and message values

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY] | [CAPABILITY]                   |
| :-----: | :--------------------------- | :------------ | :----------------------------- |
|  [01]   | `MqttClientOptions`          | class         | full connection configuration  |
|  [02]   | `MqttClientTcpOptions`       | class         | TCP endpoint and protocol      |
|  [03]   | `MqttClientTlsOptions`       | class         | TLS certificate and validation |
|  [04]   | `MqttClientWebSocketOptions` | class         | WebSocket URI and proxy        |
|  [05]   | `MqttClientSubscribeOptions` | class         | subscribe topic filter set     |
|  [06]   | `MqttClientCredentials`      | class         | username and password pair     |
|  [07]   | `MqttApplicationMessage`     | class         | topic, payload, QoS, retain    |
|  [08]   | `MqttTopicFilter`            | class         | topic, QoS, no-local, retain   |

[PUBLIC_TYPE_SCOPE]: result and event-arg values

| [INDEX] | [SYMBOL]                                  | [TYPE_FAMILY] | [CAPABILITY]                    |
| :-----: | :---------------------------------------- | :------------ | :------------------------------ |
|  [01]   | `MqttClientConnectResult`                 | class         | CONNACK reason and session flag |
|  [02]   | `MqttClientPublishResult`                 | class         | PUBACK reason and packet id     |
|  [03]   | `MqttClientSubscribeResult`               | class         | SUBACK per-filter items         |
|  [04]   | `MqttClientUnsubscribeResult`             | class         | UNSUBACK per-topic items        |
|  [05]   | `MqttClientSubscribeResultItem`           | class         | granted QoS per topic filter    |
|  [06]   | `MqttApplicationMessageReceivedEventArgs` | class         | inbound message and ack control |
|  [07]   | `MqttClientConnectedEventArgs`            | class         | connected connect-result carry  |
|  [08]   | `MqttClientDisconnectedEventArgs`         | class         | disconnect reason and exception |
|  [09]   | `MqttClientConnectingEventArgs`           | class         | options at connect attempt      |

[PUBLIC_TYPE_SCOPE]: protocol enums

| [INDEX] | [SYMBOL]                             | [TYPE_FAMILY] | [CAPABILITY]                   |
| :-----: | :----------------------------------- | :------------ | :----------------------------- |
|  [01]   | `Protocol.MqttQualityOfServiceLevel` | enum          | at-most/least/exactly-once     |
|  [02]   | `Protocol.MqttRetainHandling`        | enum          | retained delivery on subscribe |
|  [03]   | `MqttClientConnectResultCode`        | enum          | CONNACK reason codes           |
|  [04]   | `MqttClientPublishReasonCode`        | enum          | PUBACK reason codes            |
|  [05]   | `MqttClientSubscribeResultCode`      | enum          | SUBACK granted/error codes     |
|  [06]   | `MqttClientUnsubscribeResultCode`    | enum          | UNSUBACK reason codes          |
|  [07]   | `Formatter.MqttProtocolVersion`      | enum          | V310, V311, V500 selector      |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `MqttClientFactory` construction

| [INDEX] | [SURFACE]                                | [SHAPE] | [CAPABILITY]                         |
| :-----: | :--------------------------------------- | :------ | :----------------------------------- |
|  [01]   | `CreateMqttClient()`                     | factory | default `IMqttClient` instance       |
|  [02]   | `CreateMqttClient(logger)`               | factory | logger-injected client               |
|  [03]   | `CreateLowLevelMqttClient()`             | factory | manual packet `ILowLevelMqttClient`  |
|  [04]   | `CreateClientOptionsBuilder()`           | factory | `MqttClientOptionsBuilder`           |
|  [05]   | `CreateApplicationMessageBuilder()`      | factory | `MqttApplicationMessageBuilder`      |
|  [06]   | `CreateSubscribeOptionsBuilder()`        | factory | `MqttClientSubscribeOptionsBuilder`  |
|  [07]   | `CreateTopicFilterBuilder()`             | factory | `MqttTopicFilterBuilder`             |
|  [08]   | `CreateClientDisconnectOptionsBuilder()` | factory | `MqttClientDisconnectOptionsBuilder` |

[ENTRYPOINT_SCOPE]: `IMqttClient` operations and events

| [INDEX] | [SURFACE]                              | [SHAPE]  | [CAPABILITY]                  |
| :-----: | :------------------------------------- | :------- | :---------------------------- |
|  [01]   | `ConnectAsync(options, ct)`            | instance | `MqttClientConnectResult`     |
|  [02]   | `DisconnectAsync(options, ct)`         | instance | graceful DISCONNECT           |
|  [03]   | `PingAsync(ct)`                        | instance | PINGREQ keep-alive            |
|  [04]   | `PublishAsync(message, ct)`            | instance | `MqttClientPublishResult`     |
|  [05]   | `SubscribeAsync(options, ct)`          | instance | `MqttClientSubscribeResult`   |
|  [06]   | `UnsubscribeAsync(options, ct)`        | instance | `MqttClientUnsubscribeResult` |
|  [07]   | `ApplicationMessageReceivedAsync`      | instance | inbound message handler       |
|  [08]   | `ConnectedAsync` / `DisconnectedAsync` | instance | session lifecycle handlers    |
|  [09]   | `ConnectingAsync`                      | instance | pre-connect handler           |
|  [10]   | `IsConnected` / `Options`              | property | connection state and options  |

[ENTRYPOINT_SCOPE]: `MqttClientExtensions` convenience operations

| [INDEX] | [SURFACE]                                               | [SHAPE] | [CAPABILITY]                |
| :-----: | :------------------------------------------------------ | :------ | :-------------------------- |
|  [01]   | `PublishStringAsync(topic, payload, qos, retain, ct)`   | static  | UTF-8 string publish        |
|  [02]   | `PublishBinaryAsync(topic, payload, qos, retain, ct)`   | static  | byte-sequence publish       |
|  [03]   | `PublishSequenceAsync(topic, payload, qos, retain, ct)` | static  | `ReadOnlySequence` publish  |
|  [04]   | `SubscribeAsync(topic, qos, ct)`                        | static  | single-topic subscribe      |
|  [05]   | `UnsubscribeAsync(topic, ct)`                           | static  | single-topic unsubscribe    |
|  [06]   | `ReconnectAsync(ct)`                                    | static  | reconnect with last options |
|  [07]   | `DisconnectAsync(reason, reasonString, ct)`             | static  | reason-coded disconnect     |
|  [08]   | `TryPingAsync(ct)` / `TryDisconnectAsync(reason)`       | static  | swallow-fault probe         |

[ENTRYPOINT_SCOPE]: `MqttClientOptionsBuilder` connection assembly

| [INDEX] | [SURFACE]                                                             | [SHAPE]  | [CAPABILITY]                            |
| :-----: | :-------------------------------------------------------------------- | :------- | :-------------------------------------- |
|  [01]   | `WithTcpServer(host, port, family)`                                   | instance | TCP channel endpoint                    |
|  [02]   | `WithWebSocketServer(configure)`                                      | instance | WebSocket channel endpoint              |
|  [03]   | `WithConnectionUri(uri)`                                              | instance | `mqtt`/`mqtts`/`ws`/`wss`/`unix` scheme |
|  [04]   | `WithTlsOptions(configure)`                                           | instance | TLS handshake configuration             |
|  [05]   | `WithCredentials(username, password)`                                 | instance | basic auth credentials                  |
|  [06]   | `WithEnhancedAuthentication(method, data)`                            | instance | v5 enhanced auth                        |
|  [07]   | `WithClientId(value)`                                                 | instance | client identifier                       |
|  [08]   | `WithCleanStart(value)` / `WithCleanSession(value)`                   | instance | clean session/start flag                |
|  [09]   | `WithKeepAlivePeriod(value)` / `WithNoKeepAlive()`                    | instance | keep-alive period                       |
|  [10]   | `WithProtocolVersion(value)`                                          | instance | `MqttProtocolVersion` selector          |
|  [11]   | `WithSessionExpiryInterval(seconds)`                                  | instance | v5 session expiry                       |
|  [12]   | `WithTimeout(value)`                                                  | instance | socket-level timeout                    |
|  [13]   | `WithWillTopic` / `WithWillPayload` / `WithWillQualityOfServiceLevel` | instance | last-will message                       |
|  [14]   | `WithUserProperty(name, value)`                                       | instance | v5 user property, byte-payload value    |
|  [15]   | `Build()`                                                             | instance | `MqttClientOptions` value               |

[ENTRYPOINT_SCOPE]: `MqttApplicationMessageBuilder` payload assembly

| [INDEX] | [SURFACE]                                                | [SHAPE]  | [CAPABILITY]                   |
| :-----: | :------------------------------------------------------- | :------- | :----------------------------- |
|  [01]   | `WithTopic(topic)`                                       | instance | publish topic                  |
|  [02]   | `WithTopicAlias(alias)`                                  | instance | v5 topic alias                 |
|  [03]   | `WithPayload(payload)`                                   | instance | string/byte/stream/sequence    |
|  [04]   | `WithPayloadSegment(segment)`                            | instance | `ReadOnlyMemory` payload       |
|  [05]   | `WithQualityOfServiceLevel(qos)`                         | instance | QoS level                      |
|  [06]   | `WithRetainFlag(value)`                                  | instance | retained-message flag          |
|  [07]   | `WithContentType(contentType)`                           | instance | v5 content type                |
|  [08]   | `WithCorrelationData(data)` / `WithResponseTopic(topic)` | instance | v5 request-response            |
|  [09]   | `WithMessageExpiryInterval(seconds)`                     | instance | v5 expiry interval             |
|  [10]   | `WithPayloadFormatIndicator(indicator)`                  | instance | v5 payload format              |
|  [11]   | `WithSubscriptionIdentifier(id)`                         | instance | v5 subscription identifier     |
|  [12]   | `WithUserProperty(name, value)`                          | instance | v5 user property               |
|  [13]   | `Build()`                                                | instance | `MqttApplicationMessage` value |

[ENTRYPOINT_SCOPE]: `MqttClientSubscribeOptionsBuilder` filter-set assembly

| [INDEX] | [SURFACE]                            | [SHAPE]  | [CAPABILITY]            |
| :-----: | :----------------------------------- | :------- | :---------------------- |
|  [01]   | `WithTopicFilter(topic, qos, flags)` | instance | typed topic filter      |
|  [02]   | `WithTopicFilter(builder)`           | instance | builder-composed filter |
|  [03]   | `WithSubscriptionIdentifier(id)`     | instance | v5 subscription id      |

[ENTRYPOINT_SCOPE]: `MqttTopicFilterBuilder` filter assembly

`WithTopicFilter(topic, qos, flags)` carries three flags: `noLocal` suppresses messages the same client published, `retainAsPublished` preserves the retained flag on forwarded messages, and `retainHandling` selects retained-message delivery behavior.

| [INDEX] | [SURFACE]                   | [SHAPE]  | [CAPABILITY]            |
| :-----: | :-------------------------- | :------- | :---------------------- |
|  [01]   | `WithTopic(topic)`          | instance | filter topic            |
|  [02]   | `WithAtLeastOnceQoS()`      | instance | QoS-1 shorthand         |
|  [03]   | `WithNoLocal(value)`        | instance | v5 no-local flag        |
|  [04]   | `WithRetainHandling(value)` | instance | retain-handling mode    |
|  [05]   | `Build()`                   | instance | `MqttTopicFilter` value |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `MqttClientFactory` is the single construction root for clients and builders, injecting `IMqttNetLogger` and `IMqttClientAdapterFactory`.
- `IMqttClient` is `IDisposable`; events are `Func<TEventArgs, Task>` handlers the message pump awaits inline.
- Channel is exclusive: `MqttClientOptionsBuilder.Build()` throws `InvalidOperationException` when neither TCP nor WebSocket channel is set.
- `WithConnectionUri` resolves scheme to channel and TLS — `mqtt`/`tcp` plain, `mqtts` TLS, `ws`/`wss` WebSocket, `unix` domain socket — with default ports 1883 plain and 8883 TLS.
- `WithProtocolVersion(MqttProtocolVersion.Unknown)` throws; v5 metadata fields drop silently under `MqttProtocolVersion.V311`.
- Builders are mutable fluent accumulators: every `With*` returns `this` and `Build()` materializes the immutable value.
- `MqttApplicationMessageBuilder.Build()` throws `MqttProtocolViolationException` when neither topic nor topic alias is set; `MqttTopicFilterBuilder.Build()` throws it on an empty topic; `WithSubscriptionIdentifier(0)` throws it on the subscribe builder.
- Results carry MQTT reason codes, never exceptions: `MqttClientPublishResult.IsSuccess` is true for `Success` and `NoMatchingSubscribers`, and `MqttClientConnectResult.ResultCode` is `MqttClientConnectResultCode` with `Success = 0` and every error code an MQTT v5 reason code (`128`+).
- `MqttClientSubscribeResult.Items` carries one `MqttClientSubscribeResultItem` per filter with a granted-or-error `MqttClientSubscribeResultCode`.
- `MqttApplicationMessageReceivedEventArgs.AutoAcknowledge` defaults true; set false with `ProcessingFailed` to suppress the ACK.

[STACKING]:
- `api-modbus.md`/`api-bacnet.md`: an inbound MQTT message decodes to one `ExternalValue` at the same boundary the poll transports use, but the `mqtt` row binds `ReadShape.Subscribe` broker push where Modbus and MTConnect bind `ReadShape.Poll` — one decode boundary, the observation crossing as a wire row.
- within-lib: the `mqtt` row is one `ExternalTransport.Mqtt` `[SmartEnum<string>]` case and one `TransportRow` (`ReadShape.Subscribe`, `Writable: true`, an `OutboundHop.ServerStream` hop), the client's own auto-reconnect the per-row retry.
- within-lib: `MqttLane` holds the factory-built `IMqttClient` whose `ApplicationMessageReceivedAsync` callback decodes `MqttApplicationMessage.Payload` (`ReadOnlySequence<byte>`) and `TryWrite`s one `ExternalValue` into the bounded lane; `PublishAsync` over `MqttApplicationMessageBuilder` rides the write arm.
- within-lib: `LiveClient.Mqtt(IMqttClient)` seats the held connection in the one `Atom<Gate>` token-gated cell every protocol shares, so a reconnect replaces the whole cell; `MqttLane.Write` threads `TraceContext.Inject` over the message builder before `Build()` and the receive pump continues the propagated context through the `MqttRuntime.Properties` getter, so broker-hop trace continuity is the adapter's.

[LOCAL_ADMISSION]:
- AppHost composes the wire through `MqttClientFactory.CreateMqttClient`, never a direct `MqttClient` construction.
- Inbound payloads arrive as `ReadOnlySequence<byte>` on `MqttApplicationMessage.Payload`; decode at the boundary, never re-buffer per handler.
- QoS, retain, last-will, and session-expiry are `TransportRow` policy columns, never new transports or cases.
- MQTT reason codes map to typed AppHost receipts at the edge; `MqttProtocolViolationException` and `InvalidOperationException` never cross the outbound boundary.

[RAIL_LAW]:
- Package: `MQTTnet`
- Owns: MQTT `MqttProtocolVersion.V311`/v5 broker-client transport, builder-composed message assembly, and reason-code results as one outbound live-wire transport row
- Accept: a factory-built `IMqttClient`, builder-composed options, subscribe push decoded to `ExternalValue` at the boundary, and `CancellationToken`-scoped operations
- Reject: a direct `MqttClient` instantiation, a hand-rolled MQTT packet framer or second poller, and exception-driven publish/subscribe flow

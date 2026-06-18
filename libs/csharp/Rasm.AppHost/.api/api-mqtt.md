# [RASM_APPHOST_API_MQTT]

`MQTTnet` supplies an MQTT v3.1.1 and v5 client over TCP, TLS, WebSocket, and Unix-socket channels, with `MqttClientFactory` construction, fluent `MqttClientOptionsBuilder`/`MqttApplicationMessageBuilder`/`MqttTopicFilterBuilder` shapes, `IMqttClient` connect/publish/subscribe operations, async event delivery, and reason-code-typed connect, publish, subscribe, and unsubscribe results for the AppHost live-external industrial wire.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `MQTTnet`
- package: `MQTTnet`
- assembly: `MQTTnet`
- namespace: `MQTTnet`
- asset: runtime library
- rail: outbound

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: client, factory, and extension surfaces
- rail: outbound

| [INDEX] | [SYMBOL]                         | [TYPE_FAMILY]     | [RAIL]                           |
| :-----: | :------------------------------- | :---------------- | :------------------------------- |
|   [1]   | `IMqttClient`                    | client contract   | connect, publish, subscribe      |
|   [2]   | `MqttClientFactory`              | factory           | client and builder construction  |
|   [3]   | `MqttClientExtensions`           | extension methods | string/binary publish, reconnect |
|   [4]   | `IMqttClientChannelOptions`      | channel marker    | TCP/WebSocket channel selector   |
|   [5]   | `IMqttClientCredentialsProvider` | credentials       | username and password provider   |

[PUBLIC_TYPE_SCOPE]: fluent builders
- rail: outbound

| [INDEX] | [SYMBOL]                              | [TYPE_FAMILY] | [RAIL]                             |
| :-----: | :------------------------------------ | :------------ | :--------------------------------- |
|   [1]   | `MqttClientOptionsBuilder`            | options       | connection options assembly        |
|   [2]   | `MqttClientTlsOptionsBuilder`         | options       | TLS channel options assembly       |
|   [3]   | `MqttClientWebSocketOptionsBuilder`   | options       | WebSocket channel options assembly |
|   [4]   | `MqttApplicationMessageBuilder`       | message       | publish payload assembly           |
|   [5]   | `MqttClientSubscribeOptionsBuilder`   | options       | subscribe filter set assembly      |
|   [6]   | `MqttClientUnsubscribeOptionsBuilder` | options       | unsubscribe topic set assembly     |
|   [7]   | `MqttClientDisconnectOptionsBuilder`  | options       | graceful disconnect assembly       |
|   [8]   | `MqttTopicFilterBuilder`              | filter        | single topic filter assembly       |

[PUBLIC_TYPE_SCOPE]: options and message values
- rail: outbound

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY] | [RAIL]                         |
| :-----: | :--------------------------- | :------------ | :----------------------------- |
|   [1]   | `MqttClientOptions`          | options value | full connection configuration  |
|   [2]   | `MqttClientTcpOptions`       | options value | TCP endpoint and protocol      |
|   [3]   | `MqttClientTlsOptions`       | options value | TLS certificate and validation |
|   [4]   | `MqttClientWebSocketOptions` | options value | WebSocket URI and proxy        |
|   [5]   | `MqttClientSubscribeOptions` | options value | subscribe topic filter set     |
|   [6]   | `MqttClientCredentials`      | credentials   | username and password pair     |
|   [7]   | `MqttApplicationMessage`     | message value | topic, payload, QoS, retain    |
|   [8]   | `MqttTopicFilter`            | filter value  | topic, QoS, no-local, retain   |

[PUBLIC_TYPE_SCOPE]: result and event-arg values
- rail: outbound

| [INDEX] | [SYMBOL]                                  | [TYPE_FAMILY] | [RAIL]                          |
| :-----: | :---------------------------------------- | :------------ | :------------------------------ |
|   [1]   | `MqttClientConnectResult`                 | result value  | CONNACK reason and session flag |
|   [2]   | `MqttClientPublishResult`                 | result value  | PUBACK reason and packet id     |
|   [3]   | `MqttClientSubscribeResult`               | result value  | SUBACK per-filter items         |
|   [4]   | `MqttClientUnsubscribeResult`             | result value  | UNSUBACK per-topic items        |
|   [5]   | `MqttClientSubscribeResultItem`           | result item   | granted QoS per topic filter    |
|   [6]   | `MqttApplicationMessageReceivedEventArgs` | event args    | inbound message and ack control |
|   [7]   | `MqttClientConnectedEventArgs`            | event args    | connected connect-result carry  |
|   [8]   | `MqttClientDisconnectedEventArgs`         | event args    | disconnect reason and exception |
|   [9]   | `MqttClientConnectingEventArgs`           | event args    | options at connect attempt      |

[PUBLIC_TYPE_SCOPE]: protocol enums
- rail: outbound

| [INDEX] | [SYMBOL]                             | [TYPE_FAMILY] | [RAIL]                          |
| :-----: | :----------------------------------- | :------------ | :------------------------------ |
|   [1]   | `Protocol.MqttQualityOfServiceLevel` | QoS enum      | at-most/least/exactly-once      |
|   [2]   | `Protocol.MqttRetainHandling`        | retain enum   | retained delivery on subscribe  |
|   [3]   | `MqttClientConnectResultCode`        | reason enum   | CONNACK reason codes            |
|   [4]   | `MqttClientPublishReasonCode`        | reason enum   | PUBACK reason codes             |
|   [5]   | `MqttClientSubscribeResultCode`      | reason enum   | SUBACK granted/error codes      |
|   [6]   | `MqttClientUnsubscribeResultCode`    | reason enum   | UNSUBACK reason codes           |
|   [7]   | `Formatter.MqttProtocolVersion`      | version enum  | v3.1.0, v3.1.1, v5.0.0 selector |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `MqttClientFactory` construction
- rail: outbound

| [INDEX] | [SURFACE]                                | [ENTRY_FAMILY]  | [RAIL]                               |
| :-----: | :--------------------------------------- | :-------------- | :----------------------------------- |
|   [1]   | `CreateMqttClient()`                     | client factory  | default `IMqttClient` instance       |
|   [2]   | `CreateMqttClient(logger)`               | client factory  | logger-injected client               |
|   [3]   | `CreateLowLevelMqttClient()`             | client factory  | manual packet `ILowLevelMqttClient`  |
|   [4]   | `CreateClientOptionsBuilder()`           | builder factory | `MqttClientOptionsBuilder`           |
|   [5]   | `CreateApplicationMessageBuilder()`      | builder factory | `MqttApplicationMessageBuilder`      |
|   [6]   | `CreateSubscribeOptionsBuilder()`        | builder factory | `MqttClientSubscribeOptionsBuilder`  |
|   [7]   | `CreateTopicFilterBuilder()`             | builder factory | `MqttTopicFilterBuilder`             |
|   [8]   | `CreateClientDisconnectOptionsBuilder()` | builder factory | `MqttClientDisconnectOptionsBuilder` |

[ENTRYPOINT_SCOPE]: `IMqttClient` operations and events
- rail: outbound

| [INDEX] | [SURFACE]                              | [ENTRY_FAMILY] | [RAIL]                        |
| :-----: | :------------------------------------- | :------------- | :---------------------------- |
|   [1]   | `ConnectAsync(options, ct)`            | session op     | `MqttClientConnectResult`     |
|   [2]   | `DisconnectAsync(options, ct)`         | session op     | graceful DISCONNECT           |
|   [3]   | `PingAsync(ct)`                        | session op     | PINGREQ keep-alive            |
|   [4]   | `PublishAsync(message, ct)`            | message op     | `MqttClientPublishResult`     |
|   [5]   | `SubscribeAsync(options, ct)`          | message op     | `MqttClientSubscribeResult`   |
|   [6]   | `UnsubscribeAsync(options, ct)`        | message op     | `MqttClientUnsubscribeResult` |
|   [7]   | `ApplicationMessageReceivedAsync`      | event          | inbound message handler       |
|   [8]   | `ConnectedAsync` / `DisconnectedAsync` | event          | session lifecycle handlers    |
|   [9]   | `ConnectingAsync`                      | event          | pre-connect handler           |
|  [10]   | `IsConnected` / `Options`              | state          | connection state and options  |

[ENTRYPOINT_SCOPE]: `MqttClientExtensions` convenience operations
- rail: outbound

| [INDEX] | [SURFACE]                                               | [ENTRY_FAMILY] | [RAIL]                      |
| :-----: | :------------------------------------------------------ | :------------- | :-------------------------- |
|   [1]   | `PublishStringAsync(topic, payload, qos, retain, ct)`   | message op     | UTF-8 string publish        |
|   [2]   | `PublishBinaryAsync(topic, payload, qos, retain, ct)`   | message op     | byte-sequence publish       |
|   [3]   | `PublishSequenceAsync(topic, payload, qos, retain, ct)` | message op     | `ReadOnlySequence` publish  |
|   [4]   | `SubscribeAsync(topic, qos, ct)`                        | message op     | single-topic subscribe      |
|   [5]   | `UnsubscribeAsync(topic, ct)`                           | message op     | single-topic unsubscribe    |
|   [6]   | `ReconnectAsync(ct)`                                    | session op     | reconnect with last options |
|   [7]   | `DisconnectAsync(reason, reasonString, ct)`             | session op     | reason-coded disconnect     |
|   [8]   | `TryPingAsync(ct)` / `TryDisconnectAsync(reason)`       | session op     | swallow-fault probe         |

[ENTRYPOINT_SCOPE]: `MqttClientOptionsBuilder` connection assembly
- rail: outbound

| [INDEX] | [SURFACE]                                                             | [ENTRY_FAMILY] | [RAIL]                                  |
| :-----: | :-------------------------------------------------------------------- | :------------- | :-------------------------------------- |
|   [1]   | `WithTcpServer(host, port, family)`                                   | channel        | TCP endpoint                            |
|   [2]   | `WithWebSocketServer(configure)`                                      | channel        | WebSocket endpoint                      |
|   [3]   | `WithConnectionUri(uri)`                                              | channel        | `mqtt`/`mqtts`/`ws`/`wss`/`unix` scheme |
|   [4]   | `WithTlsOptions(configure)`                                           | security       | TLS handshake configuration             |
|   [5]   | `WithCredentials(username, password)`                                 | security       | basic auth credentials                  |
|   [6]   | `WithEnhancedAuthentication(method, data)`                            | security       | v5 enhanced auth                        |
|   [7]   | `WithClientId(value)`                                                 | session        | client identifier                       |
|   [8]   | `WithCleanStart(value)` / `WithCleanSession(value)`                   | session        | clean session/start flag                |
|   [9]   | `WithKeepAlivePeriod(value)` / `WithNoKeepAlive()`                    | session        | keep-alive period                       |
|  [10]   | `WithProtocolVersion(value)`                                          | session        | `MqttProtocolVersion` selector          |
|  [11]   | `WithSessionExpiryInterval(seconds)`                                  | session        | v5 session expiry                       |
|  [12]   | `WithTimeout(value)`                                                  | session        | socket-level timeout                    |
|  [13]   | `WithWillTopic` / `WithWillPayload` / `WithWillQualityOfServiceLevel` | will           | last-will message                       |
|  [14]   | `WithUserProperty(name, value)`                                       | session        | v5 user property                        |
|  [15]   | `Build()`                                                             | terminal       | `MqttClientOptions` value               |

[ENTRYPOINT_SCOPE]: `MqttApplicationMessageBuilder` payload assembly
- rail: outbound

| [INDEX] | [SURFACE]                                                | [ENTRY_FAMILY] | [RAIL]                         |
| :-----: | :------------------------------------------------------- | :------------- | :----------------------------- |
|   [1]   | `WithTopic(topic)`                                       | addressing     | publish topic                  |
|   [2]   | `WithTopicAlias(alias)`                                  | addressing     | v5 topic alias                 |
|   [3]   | `WithPayload(payload)`                                   | payload        | string/byte/stream/sequence    |
|   [4]   | `WithPayloadSegment(segment)`                            | payload        | `ReadOnlyMemory` payload       |
|   [5]   | `WithQualityOfServiceLevel(qos)`                         | delivery       | QoS level                      |
|   [6]   | `WithRetainFlag(value)`                                  | delivery       | retained-message flag          |
|   [7]   | `WithContentType(contentType)`                           | metadata       | v5 content type                |
|   [8]   | `WithCorrelationData(data)` / `WithResponseTopic(topic)` | metadata       | v5 request-response            |
|   [9]   | `WithMessageExpiryInterval(seconds)`                     | metadata       | v5 expiry interval             |
|  [10]   | `WithPayloadFormatIndicator(indicator)`                  | metadata       | v5 payload format              |
|  [11]   | `WithSubscriptionIdentifier(id)`                         | metadata       | v5 subscription identifier     |
|  [12]   | `WithUserProperty(name, value)`                          | metadata       | v5 user property               |
|  [13]   | `Build()`                                                | terminal       | `MqttApplicationMessage` value |

[ENTRYPOINT_SCOPE]: subscribe and topic-filter assembly
- rail: outbound

| [INDEX] | [SURFACE]                                                                                                   | [ENTRY_FAMILY] | [RAIL]                  |
| :-----: | :---------------------------------------------------------------------------------------------------------- | :------------- | :---------------------- |
|   [1]   | `MqttClientSubscribeOptionsBuilder.WithTopicFilter(topic, qos, noLocal, retainAsPublished, retainHandling)` | filter         | typed topic filter      |
|   [2]   | `MqttClientSubscribeOptionsBuilder.WithTopicFilter(builder)`                                                | filter         | builder-composed filter |
|   [3]   | `MqttClientSubscribeOptionsBuilder.WithSubscriptionIdentifier(id)`                                          | filter         | v5 subscription id      |
|   [4]   | `MqttTopicFilterBuilder.WithTopic(topic)`                                                                   | filter         | filter topic            |
|   [5]   | `MqttTopicFilterBuilder.WithAtLeastOnceQoS()`                                                               | filter         | QoS-1 shorthand         |
|   [6]   | `MqttTopicFilterBuilder.WithNoLocal(value)`                                                                 | filter         | v5 no-local flag        |
|   [7]   | `MqttTopicFilterBuilder.WithRetainHandling(value)`                                                          | filter         | retain-handling mode    |
|   [8]   | `MqttTopicFilterBuilder.Build()`                                                                            | terminal       | `MqttTopicFilter` value |

## [4]-[IMPLEMENTATION_LAW]

[CLIENT_TOPOLOGY]:
- `MqttClientFactory` is the single construction root for clients and builders; it injects `IMqttNetLogger` and `IMqttClientAdapterFactory`.
- `IMqttClient` is `IDisposable`; events are `Func<TEventArgs, Task>` async handlers awaited inline by the message pump.
- channel is exclusive: `MqttClientOptionsBuilder.Build()` throws `InvalidOperationException` when neither TCP nor WebSocket channel is set.
- `WithConnectionUri` resolves scheme to channel and TLS: `mqtt`/`tcp` plain, `mqtts` TLS, `ws`/`wss` WebSocket, `unix` domain socket; default ports are 1883 plain and 8883 TLS.
- protocol version: `WithProtocolVersion(MqttProtocolVersion.Unknown)` throws; v5 metadata fields are silently dropped under v3.1.1.

[BUILDER_LAW]:
- builders are mutable fluent accumulators; every `With*` returns `this` and `Build()` materializes the immutable value.
- `MqttApplicationMessageBuilder.Build()` throws `MqttProtocolViolationException` when neither topic nor topic alias is set.
- `MqttTopicFilterBuilder.Build()` throws `MqttProtocolViolationException` when the topic is empty.
- `WithUserProperty(string, string)` and `WithWillUserProperty(string, string)` are `[Obsolete]`; prefer the `ReadOnlyMemory<byte>` and `ArraySegment<byte>` overloads.
- `WithSubscriptionIdentifier(0)` on the subscribe builder throws `MqttProtocolViolationException`.

[RESULT_LAW]:
- results carry MQTT reason codes, never exceptions: `MqttClientPublishResult.IsSuccess` is true for `Success` and `NoMatchingSubscribers`.
- `MqttClientConnectResult.ResultCode` is `MqttClientConnectResultCode`; `Success = 0`, all error codes are MQTT v5 reason codes (`128`+).
- `MqttClientSubscribeResult.Items` carries one `MqttClientSubscribeResultItem` per filter with a granted-or-error `MqttClientSubscribeResultCode`.
- `MqttApplicationMessageReceivedEventArgs.AutoAcknowledge` defaults to true; set false plus `ProcessingFailed` to suppress the ACK.

[LOCAL_ADMISSION]:
- AppHost composes the wire through `MqttClientFactory.CreateMqttClient`; never instantiate `MqttClient` directly.
- inbound payloads arrive as `ReadOnlySequence<byte>` on `MqttApplicationMessage.Payload`; decode at the boundary, never re-buffer per handler.
- map MQTT reason codes to typed AppHost receipts at the edge; `MqttProtocolViolationException` and `InvalidOperationException` never cross the outbound boundary.

[RAIL_LAW]:
- Package: `MQTTnet`
- Owns: MQTT v3.1.1/v5 client transport, message assembly, and reason-code results
- Accept: factory-built clients, builder-composed options, `CancellationToken`-scoped operations
- Reject: direct client instantiation, hand-rolled MQTT packet framing, exception-driven publish/subscribe flow

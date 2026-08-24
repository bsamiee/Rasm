# [DOTNET_API_MQTT]

`MQTTnet` owns legacy and v5 broker-client transport in BOTH directions across three consuming folders: `MqttClientFactory` mints one `IMqttClient` per leg, builder-composed options carry session and channel policy, `PublishAsync` folds its PUBACK to a reason-code result, and `SubscribeAsync` beside `ApplicationMessageReceivedAsync` carries every delivery. `MqttProtocolVersion.V500` opens the `UserProperties` plane the W3C `traceparent`/`tracestate` pair rides, so a publish joins the producing trace and a delivery hands its parent to the consuming bracket.

Rails: `Rasm.AppHost` binds the outbound live-wire `mqtt` transport row, `Rasm.Persistence` the `mqtt` binding-row deliver leg, and `Rasm.Compute` the CloudEvents-decoded sensor-ingest pump.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `MQTTnet`
- package: `MQTTnet` (MIT)
- assembly: `MQTTnet`
- namespace: `MQTTnet`, `MQTTnet.Protocol`, `MQTTnet.Packets`, `MQTTnet.Formatter`
- asset: pure-managed runtime library; control-packet framing rides the client's own socket or WebSocket channel
- rail: outbound live-wire, egress-sink, sensor-ingest

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

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY] | [CAPABILITY]                     |
| :-----: | :--------------------------- | :------------ | :------------------------------- |
|  [01]   | `MqttClientOptions`          | class         | full connection configuration    |
|  [02]   | `MqttClientTcpOptions`       | class         | TCP endpoint and protocol        |
|  [03]   | `MqttClientTlsOptions`       | class         | TLS certificate and validation   |
|  [04]   | `MqttClientWebSocketOptions` | class         | WebSocket URI and proxy          |
|  [05]   | `MqttClientSubscribeOptions` | class         | subscribe topic filter set       |
|  [06]   | `MqttClientCredentials`      | class         | username and password pair       |
|  [07]   | `MqttApplicationMessage`     | class         | topic, payload sequence, QoS, v5 |
|  [08]   | `Packets.MqttTopicFilter`    | class         | topic, QoS, no-local, retain     |
|  [09]   | `Packets.MqttUserProperty`   | class         | v5 `Name`/`ValueBuffer` pair     |
|  [10]   | `MqttUserPropertyExtensions` | class         | UTF-8 value read off an entry    |

[PUBLIC_TYPE_SCOPE]: result and event-arg values

| [INDEX] | [SYMBOL]                                  | [TYPE_FAMILY] | [CAPABILITY]                       |
| :-----: | :---------------------------------------- | :------------ | :--------------------------------- |
|  [01]   | `MqttClientConnectResult`                 | class         | CONNACK, session flag, v5 limits   |
|  [02]   | `MqttClientPublishResult`                 | class         | PUBACK reason, reason string, id   |
|  [03]   | `MqttClientSubscribeResult`               | class         | SUBACK per-filter items            |
|  [04]   | `MqttClientUnsubscribeResult`             | class         | UNSUBACK per-topic items           |
|  [05]   | `MqttClientSubscribeResultItem`           | class         | granted QoS per topic filter       |
|  [06]   | `MqttApplicationMessageReceivedEventArgs` | class         | delivery, ack control, reason slot |
|  [07]   | `MqttClientConnectedEventArgs`            | class         | connected connect-result carry     |
|  [08]   | `MqttClientDisconnectedEventArgs`         | class         | disconnect reason and exception    |
|  [09]   | `MqttClientConnectingEventArgs`           | class         | options at connect attempt         |
|  [10]   | `MqttClientUnsubscribeResultItem`         | class         | per-topic UNSUBACK reason code     |
|  [11]   | `MqttClientPublishResultFactory`          | class         | PUBACK fold to a publish result    |
|  [12]   | `MqttClientSubscribeResultFactory`        | class         | SUBACK fold to per-filter items    |
|  [13]   | `MqttClientUnsubscribeResultFactory`      | class         | UNSUBACK fold to per-topic items   |

[PUBLIC_TYPE_SCOPE]: protocol enums

| [INDEX] | [SYMBOL]                                   | [TYPE_FAMILY] | [CAPABILITY]                    |
| :-----: | :----------------------------------------- | :------------ | :------------------------------ |
|  [01]   | `Protocol.MqttQualityOfServiceLevel`       | enum          | at-most/least/exactly-once      |
|  [02]   | `Protocol.MqttRetainHandling`              | enum          | retained delivery on subscribe  |
|  [03]   | `MqttClientConnectResultCode`              | enum          | CONNACK reason codes            |
|  [04]   | `MqttClientPublishReasonCode`              | enum          | PUBACK reason codes             |
|  [05]   | `MqttClientSubscribeResultCode`            | enum          | SUBACK granted/error codes      |
|  [06]   | `MqttClientUnsubscribeResultCode`          | enum          | UNSUBACK reason codes           |
|  [07]   | `MqttApplicationMessageReceivedReasonCode` | enum          | PUBACK reason a handler returns |
|  [08]   | `Formatter.MqttProtocolVersion`            | enum          | V310, V311, V500 selector       |

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

| [INDEX] | [SURFACE]                                               | [SHAPE]  | [CAPABILITY]                  |
| :-----: | :------------------------------------------------------ | :------- | :---------------------------- |
|  [01]   | `ConnectAsync(options, ct)`                             | instance | `MqttClientConnectResult`     |
|  [02]   | `DisconnectAsync(options, ct)`                          | instance | graceful DISCONNECT           |
|  [03]   | `PingAsync(ct)`                                         | instance | PINGREQ keep-alive            |
|  [04]   | `PublishAsync(message, ct)`                             | instance | `MqttClientPublishResult`     |
|  [05]   | `SubscribeAsync(options, ct)`                           | instance | `MqttClientSubscribeResult`   |
|  [06]   | `UnsubscribeAsync(options, ct)`                         | instance | `MqttClientUnsubscribeResult` |
|  [07]   | `SendEnhancedAuthenticationExchangeDataAsync(data, ct)` | instance | v5 auth-exchange continuation |
|  [08]   | `ApplicationMessageReceivedAsync`                       | event    | `+=` binds, `-=` detaches     |
|  [09]   | `ConnectedAsync` / `DisconnectedAsync`                  | event    | session lifecycle handlers    |
|  [10]   | `ConnectingAsync`                                       | event    | pre-connect handler           |
|  [11]   | `InspectPacketAsync`                                    | event    | raw packet inspection hook    |
|  [12]   | `IsConnected` / `Options`                               | property | connection state and options  |

[ENTRYPOINT_SCOPE]: `MqttClientExtensions` convenience operations

| [INDEX] | [SURFACE]                                                  | [SHAPE] | [CAPABILITY]                  |
| :-----: | :--------------------------------------------------------- | :------ | :---------------------------- |
|  [01]   | `PublishStringAsync(topic, payload, qos, retain, ct)`      | static  | UTF-8 string publish          |
|  [02]   | `PublishBinaryAsync(topic, payload, qos, retain, ct)`      | static  | byte-sequence publish         |
|  [03]   | `PublishSequenceAsync(topic, payload, qos, retain, ct)`    | static  | zero-copy `ReadOnlySequence`  |
|  [04]   | `SubscribeAsync(topic, qos, ct)`                           | static  | single-topic subscribe        |
|  [05]   | `SubscribeAsync(MqttTopicFilter, ct)`                      | static  | one prebuilt filter subscribe |
|  [06]   | `UnsubscribeAsync(topic, ct)`                              | static  | single-topic unsubscribe      |
|  [07]   | `ReconnectAsync(ct)`                                       | static  | re-drive on the held options  |
|  [08]   | `DisconnectAsync(reason, reasonString, expiry, props, ct)` | static  | reason-coded teardown         |
|  [09]   | `TryPingAsync(ct)` / `TryDisconnectAsync(reason, reason)`  | static  | fault-swallowing probe        |
|  [10]   | `SendEnhancedAuthenticationExchangeDataAsync(data)`        | static  | v5 auth-exchange continuation |

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
|  [12]   | `WithRequestProblemInformation(value)`                                | instance | broker returns `ReasonString`           |
|  [13]   | `WithTimeout(value)`                                                  | instance | socket-level timeout                    |
|  [14]   | `WithWillTopic` / `WithWillPayload` / `WithWillQualityOfServiceLevel` | instance | last-will message                       |
|  [15]   | `WithUserProperty(name, value)`                                       | instance | v5 user property, byte-payload value    |
|  [16]   | `Build()`                                                             | instance | `MqttClientOptions` value               |

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

[ENTRYPOINT_SCOPE]: v5 user-property carriage

Receive reads the entry buffer, never the string property: `MqttUserProperty.Value` and the `(string, string)` constructor both carry `[Obsolete]` pointing at the buffer pair below, and the collection is a settable `List<MqttUserProperty>` an inbound message hands whole, so an extraction fold walks it under an ordinal name comparison and decodes each hit once.

| [INDEX] | [SURFACE]                                                         | [SHAPE]   | [CAPABILITY]                    |
| :-----: | :---------------------------------------------------------------- | :-------- | :------------------------------ |
|  [01]   | `MqttApplicationMessage.UserProperties -> List<MqttUserProperty>` | property  | v5 entry collection, both ways  |
|  [02]   | `MqttUserProperty.Name -> string`                                 | property  | entry key, allocation-free      |
|  [03]   | `MqttUserProperty.ValueBuffer -> ReadOnlyMemory<byte>`            | property  | entry value bytes               |
|  [04]   | `MqttUserPropertyExtensions.ReadValueAsString(MqttUserProperty)`  | extension | UTF-8 decode, empty buffer maps |
|  [05]   | `MqttUserProperty(string, ReadOnlyMemory<byte>)`                  | ctor      | non-obsolete entry mint         |
|  [06]   | `MqttApplicationMessageExtensions.ConvertPayloadToString(...)`    | extension | UTF-8 payload read, null empty  |

[ENTRYPOINT_SCOPE]: delivery acknowledgement on `MqttApplicationMessageReceivedEventArgs`

| [INDEX] | [SURFACE]                                         | [SHAPE]  | [CAPABILITY]                       |
| :-----: | :------------------------------------------------ | :------- | :--------------------------------- |
|  [01]   | `ApplicationMessage` / `ClientId`                 | property | delivered message and session id   |
|  [02]   | `AutoAcknowledge`                                 | property | FALSE hands the ack to the handler |
|  [03]   | `AcknowledgeAsync(CancellationToken)`             | instance | idempotent under its latch         |
|  [04]   | `ProcessingFailed`                                | property | suppresses the ack for redelivery  |
|  [05]   | `ReasonCode`                                      | property | reason the ack carries back        |
|  [06]   | `ResponseReasonString` / `ResponseUserProperties` | property | v5 payload riding the ack packet   |
|  [07]   | `IsHandled`                                       | property | consumer-owned control flow flag   |
|  [08]   | `PacketIdentifier`                                | property | `ushort` PUBLISH packet id         |

[ENTRYPOINT_SCOPE]: `MqttClientSubscribeOptionsBuilder` filter-set assembly

| [INDEX] | [SURFACE]                            | [SHAPE]  | [CAPABILITY]                 |
| :-----: | :----------------------------------- | :------- | :--------------------------- |
|  [01]   | `WithTopicFilter(topic, qos, flags)` | instance | typed topic filter           |
|  [02]   | `WithTopicFilter(builder)`           | instance | builder-composed filter      |
|  [03]   | `WithSubscriptionIdentifier(id)`     | instance | v5 subscription id           |
|  [04]   | `Build()`                            | instance | `MqttClientSubscribeOptions` |

[ENTRYPOINT_SCOPE]: `MqttTopicFilterBuilder` filter assembly

`WithTopicFilter(topic, qos, flags)` carries three flags: `noLocal` suppresses messages the same client published, `retainAsPublished` preserves the retained flag on forwarded messages, and `retainHandling` selects retained-message delivery behavior.

| [INDEX] | [SURFACE]                   | [SHAPE]  | [CAPABILITY]            |
| :-----: | :-------------------------- | :------- | :---------------------- |
|  [01]   | `WithTopic(topic)`          | instance | filter topic            |
|  [02]   | `WithAtLeastOnceQoS()`      | instance | QoS-1 shorthand         |
|  [03]   | `WithNoLocal(value)`        | instance | v5 no-local flag        |
|  [04]   | `WithRetainHandling(value)` | instance | retain-handling mode    |
|  [05]   | `Build()`                   | instance | `MqttTopicFilter` value |

[ENTRYPOINT_SCOPE]: result value shapes

`MqttClientPublishResult` is a `public sealed class` whose `IsSuccess` computes off `ReasonCode`, never a stored flag. Subscribe and unsubscribe results carry one item per requested filter, and the item — not the result — holds the per-filter verdict.

| [INDEX] | [SURFACE]                                                    | [SHAPE]  | [CAPABILITY]                            |
| :-----: | :----------------------------------------------------------- | :------- | :-------------------------------------- |
|  [01]   | `MqttClientPublishResult.IsSuccess -> bool`                  | property | `Success` or `NoMatchingSubscribers`    |
|  [02]   | `MqttClientPublishResult.ReasonCode`                         | property | `MqttClientPublishReasonCode` verdict   |
|  [03]   | `MqttClientPublishResult.PacketIdentifier -> ushort?`        | property | null under QoS 0                        |
|  [04]   | `MqttClientPublishResult.ReasonString -> string`             | property | broker diagnostic text                  |
|  [05]   | `MqttClientPublishResult.UserProperties`                     | property | `IReadOnlyCollection<MqttUserProperty>` |
|  [06]   | `MqttClientSubscribeResult.Items`                            | property | one item per topic filter               |
|  [07]   | `MqttClientSubscribeResult.PacketIdentifier -> ushort`       | property | SUBACK packet id                        |
|  [08]   | `MqttClientSubscribeResult.ReasonString` / `.UserProperties` | property | v5 SUBACK diagnostics, get-only         |
|  [09]   | `MqttClientSubscribeResultItem.ResultCode`                   | property | `MqttClientSubscribeResultCode`         |
|  [10]   | `MqttClientSubscribeResultItem.TopicFilter`                  | property | `MqttTopicFilter` the item answers      |
|  [11]   | `MqttClientUnsubscribeResult.Items`                          | property | one item per unsubscribed topic         |
|  [12]   | `MqttClientUnsubscribeResult.UserProperties`                 | property | settable, unlike the subscribe twin     |
|  [13]   | `MqttClientUnsubscribeResultItem.ResultCode`                 | property | `MqttClientUnsubscribeResultCode`       |
|  [14]   | `MqttClientUnsubscribeResultItem.TopicFilter -> string`      | property | the topic string the item answers       |

[ENTRYPOINT_SCOPE]: reason-code rosters

Five reason-code enums share one MQTT v5 code space, so a lane column marks membership per enum: `[PUBLISH]` is `MqttClientPublishReasonCode`, `[SUBSCRIBE]` `MqttClientSubscribeResultCode`, `[UNSUBSCRIBE]` `MqttClientUnsubscribeResultCode`, `[CONNECT]` `MqttClientConnectResultCode`, `[RECEIVED]` `MqttApplicationMessageReceivedReasonCode`. Value `0` spells `Success` on four lanes and `GrantedQoS0` on the subscribe lane, and every value at `128` or above is a refusal.

| [INDEX] | [VALUE] | [MEMBER]                              | [PUBLISH] | [SUBSCRIBE] | [UNSUBSCRIBE] | [CONNECT] | [RECEIVED] |
| :-----: | :-----: | :------------------------------------ | :-------: | :---------: | :-----------: | :-------: | :--------: |
|  [01]   |    0    | `Success`                             |    [X]    |             |      [X]      |    [X]    |    [X]     |
|  [02]   |    0    | `GrantedQoS0`                         |           |     [X]     |               |           |            |
|  [03]   |    1    | `GrantedQoS1`                         |           |     [X]     |               |           |            |
|  [04]   |    2    | `GrantedQoS2`                         |           |     [X]     |               |           |            |
|  [05]   |   16    | `NoMatchingSubscribers`               |    [X]    |             |               |           |    [X]     |
|  [06]   |   17    | `NoSubscriptionExisted`               |           |             |      [X]      |           |            |
|  [07]   |   128   | `UnspecifiedError`                    |    [X]    |     [X]     |      [X]      |    [X]    |    [X]     |
|  [08]   |   129   | `MalformedPacket`                     |           |             |               |    [X]    |            |
|  [09]   |   130   | `ProtocolError`                       |           |             |               |    [X]    |            |
|  [10]   |   131   | `ImplementationSpecificError`         |    [X]    |     [X]     |      [X]      |    [X]    |    [X]     |
|  [11]   |   132   | `UnsupportedProtocolVersion`          |           |             |               |    [X]    |            |
|  [12]   |   133   | `ClientIdentifierNotValid`            |           |             |               |    [X]    |            |
|  [13]   |   134   | `BadUserNameOrPassword`               |           |             |               |    [X]    |            |
|  [14]   |   135   | `NotAuthorized`                       |    [X]    |     [X]     |      [X]      |    [X]    |    [X]     |
|  [15]   |   136   | `ServerUnavailable`                   |           |             |               |    [X]    |            |
|  [16]   |   137   | `ServerBusy`                          |           |             |               |    [X]    |            |
|  [17]   |   138   | `Banned`                              |           |             |               |    [X]    |            |
|  [18]   |   140   | `BadAuthenticationMethod`             |           |             |               |    [X]    |            |
|  [19]   |   143   | `TopicFilterInvalid`                  |           |     [X]     |      [X]      |           |            |
|  [20]   |   144   | `TopicNameInvalid`                    |    [X]    |             |               |    [X]    |    [X]     |
|  [21]   |   145   | `PacketIdentifierInUse`               |    [X]    |     [X]     |      [X]      |    [X]    |    [X]     |
|  [22]   |   146   | `PacketIdentifierNotFound`            |           |             |               |           |    [X]     |
|  [23]   |   149   | `PacketTooLarge`                      |           |             |               |    [X]    |            |
|  [24]   |   151   | `QuotaExceeded`                       |    [X]    |     [X]     |      [X]      |    [X]    |    [X]     |
|  [25]   |   153   | `PayloadFormatInvalid`                |    [X]    |             |               |    [X]    |    [X]     |
|  [26]   |   154   | `RetainNotSupported`                  |           |             |               |    [X]    |            |
|  [27]   |   155   | `QoSNotSupported`                     |           |             |               |    [X]    |            |
|  [28]   |   156   | `UseAnotherServer`                    |           |             |               |    [X]    |            |
|  [29]   |   157   | `ServerMoved`                         |           |             |               |    [X]    |            |
|  [30]   |   158   | `SharedSubscriptionsNotSupported`     |           |     [X]     |               |           |            |
|  [31]   |   159   | `ConnectionRateExceeded`              |           |             |               |    [X]    |            |
|  [32]   |   161   | `SubscriptionIdentifiersNotSupported` |           |     [X]     |               |           |            |
|  [33]   |   162   | `WildcardSubscriptionsNotSupported`   |           |     [X]     |               |           |            |

[TOPOLOGY]:
- `MqttClientFactory` is the single construction root for clients and builders, injecting `IMqttNetLogger` and `IMqttClientAdapterFactory`, and `CreateMqttClient()` mints a distinct `IMqttClient` per call, so one leg owns one connection and disposes it with the leg.
- `IMqttClient` is `IDisposable` and serializes outgoing packets internally, routing each acknowledgement to its awaiting caller by packet identifier, so concurrent publishes are safe while `ConnectAsync` and `DisconnectAsync` hold the connection alone.
- `MqttApplicationMessage.Payload` is a `ReadOnlySequence<byte>`, so a large buffered message envelope publishes over its own segments with no boundary re-buffer.
- Channel is exclusive: `MqttClientOptionsBuilder.Build()` throws `InvalidOperationException` when neither TCP nor WebSocket channel is set.
- `WithConnectionUri` resolves scheme to channel and TLS — `mqtt`/`tcp` plain, `mqtts` TLS, `ws`/`wss` WebSocket, `unix` domain socket — with default ports 1883 plain and 8883 TLS.
- `WithProtocolVersion(MqttProtocolVersion.Unknown)` throws; v5 metadata fields drop silently under `MqttProtocolVersion.V311`.
- `WithCleanStart(false)` under a non-zero `WithSessionExpiryInterval` holds in-flight QoS-1 state at the broker across a reconnect, and `MqttClientConnectResult.IsSessionPresent` reports whether it did.
- Builders are mutable fluent accumulators: every `With*` returns `this` and `Build()` materializes the immutable value.
- `MqttApplicationMessageBuilder.Build()` throws `MqttProtocolViolationException` when neither topic nor topic alias is set; `MqttTopicFilterBuilder.Build()` throws it on an empty topic; `WithSubscriptionIdentifier(0)` throws it on the subscribe builder, so an identifier is either omitted or non-zero at the composing row.
- Results carry MQTT reason codes, never exceptions: `PublishAsync` throws for LOCAL faults alone — a tripped token, an invalid topic, a disposed or unconnected client, a feature `ValidateFeatures` refuses — because `MqttClientPublishResultFactory.Create` casts the PUBACK reason straight through, and `ConnectAsync` likewise RETURNS a failed `MqttClientConnectResult` rather than throwing.
- QoS 0 answers a static success result carrying a null `PacketIdentifier`; QoS 2 folds PUBREC beside PUBCOMP, mapping a `PacketIdentifierNotFound` PUBCOMP to `UnspecifiedError` and a null packet pair to `ImplementationSpecificError`.
- `MqttClientSubscribeResultFactory` and `MqttClientUnsubscribeResultFactory` throw `MqttProtocolViolationException` ONLY where the SUBACK/UNSUBACK reason-code count mismatches the requested filter count; every per-filter verdict rides its returned item.
- `MqttClientOptions` defaults the composing row inherits: `ProtocolVersion = V500`, `RequestProblemInformation = true` so `ReasonString` arrives, `ValidateFeatures = true`, `Timeout` 100 s, `KeepAlivePeriod` 15 s, `CleanSession = true`.
- TRAP: `MqttClientPublishResult.IsSuccess` is TRUE for `NoMatchingSubscribers`, so a publish that reached NO subscriber reads as success. Any fence proving delivery to at least one subscriber branches on `ReasonCode`, never on `IsSuccess`.
- `ApplicationMessageReceivedAsync` runs its handler on the client's own receive loop, so a handler that blocks stalls every later delivery on that session — a consumer bridges onto its own queue and returns.
- `AutoAcknowledge` defaults TRUE and acks BEFORE the handler's outcome is known, so a consumer shedding a delivery under that default acks a message it dropped; FALSE with `AcknowledgeAsync` on the accepted path alone leaves a shed QoS 1/2 delivery for redelivery, and `ProcessingFailed` suppresses the packet outright.
- `MqttUserProperty.Value` and the `(string, string)` ctor are `[Obsolete]` at the admitted pin — `ValueBuffer` beside `ReadValueAsString()` is the live read, and that extension answers `string.Empty` rather than null for an empty buffer.

[STACKING]:
- `api-cloudevents`(`api-cloudevents.md`): the MQTT binding is branch-owned over this carrier — a leg frames through `EventEnvelope.Encode`, writes the body onto `MqttApplicationMessage.Payload`, and carries the attributes as UNPREFIXED v5 User Properties in binary mode; `Egress.Envelope` mints the `CloudEvent` `id` MQTT carries as its sole replay key, so receiver-side dedup on that id absorbs every held-cursor re-drive.
- `api-modbus.md`/`api-bacnet.md`: an inbound MQTT message decodes to one `ExternalValue` at the same boundary the poll transports use, but the `mqtt` row binds `ReadShape.Subscribe` broker push where Modbus and MTConnect bind `ReadShape.Poll` — one decode boundary, the observation crossing as a wire row.
- `Rasm.AppHost`: `ExternalTransport.Mqtt` seats the `mqtt` row as one `[SmartEnum<string>]` case with one `TransportRow` (`ReadShape.Subscribe`, `Writable: true`, an `OutboundHop.ServerStream` hop) whose per-row retry is the client's own auto-reconnect; `MqttLane` holds the factory-built client and `TryWrite`s one decoded `ExternalValue` into the bounded lane, `LiveClient.Mqtt(IMqttClient)` seats the connection in the shared `Atom<Gate>` cell, and `MqttLane.Write` threads `TraceContext.Inject` over the message builder before `Build()` while the receive pump continues the propagated context through `TraceContext`'s own `MqttApplicationMessage` overload, whose ordinal-matched getter decodes `ValueBuffer` through `ReadValueAsString`.
- `Rasm.Persistence`: the `mqtt` binding row binds the factory-minted client into `SinkBinding.Leg`, whose fold maps `IsSuccess` to `Persisted`, a transport ambiguity to `Indeterminate`, and a `128`+ `ReasonCode` carrying its `ReasonString` to a dead-lettering `Refused`.
- `Rasm.Compute`: the MQTT `BrokerBinding` row's `Reader` feeds ONE `BrokerChannels.Decode<T>` and ONE `BrokerChannels.Pump<T>` — reading the framing off `MqttApplicationMessage.ContentType`, taking the structured leg through `Rasm/Domain/event` `EventEnvelope.Decode` over `MqttApplicationMessage.Payload` and the binary leg through the branch-owned binding over the UNPREFIXED v5 `UserProperties`, and landing `SensorReading<T>` whose causal, lag, sampling, and expiry facts project off the message envelope's own rostered attributes, so the ingest pump opens no span of its own and the composing root adopts the inbound parent. The SUBACK is DATA: `MqttClientSubscribeResult.Items` classifies per topic filter through `Refusal(topicFilter, MqttClientSubscribeResultCode)`, `QuotaExceeded` alone riding the transient arm while `NotAuthorized` and every other refusal code are terminal.

[LOCAL_ADMISSION]:
- Every folder composes the wire through `MqttClientFactory.CreateMqttClient`, never a direct `MqttClient` construction, and takes the bound client from its own composition root so no case constructs one.
- Inbound payloads arrive as `ReadOnlySequence<byte>` on `MqttApplicationMessage.Payload`; decode at the boundary, never re-buffer per handler.
- QoS, retain, last-will, and session-expiry are policy columns on the composing row, never new transports or cases.
- Settlement posture and the receiving lane's overflow policy are ONE bargain a composing row states together: a row settling on admission over a dropping lane refuses `ExactlyOnce` at its own mint, since a discarded delivery the broker already released never redelivers.
- MQTT reason codes map to typed receipts at the edge; `MqttProtocolViolationException` and `InvalidOperationException` never cross an outbound boundary.

[RAIL_LAW]:
- Package: `MQTTnet`
- Owns: legacy and v5 broker-client transport in both directions — session and channel assembly, builder-composed message assembly, the `UserProperties` tracing carrier, reason-code-typed results, and the subscribe leg beside its delivery event and acknowledgement control
- Accept: factory-minted clients, builder-composed v5 sessions and subscriptions, `CancellationToken`-scoped publish and subscribe, handler-owned acknowledgement, and results folded to a typed receipt at the boundary
- Reject: a direct `MqttClient` instantiation, a hand-rolled MQTT packet framer or second poller, exception-driven publish and subscribe flow, work inside the delivery handler, the `[Obsolete]` `MqttUserProperty.Value` read, and a raw result value crossing into an interior

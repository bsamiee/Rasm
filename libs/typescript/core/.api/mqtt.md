# [TS_CORE_API_MQTT]

`mqtt` is the MQTT.js client realizing the carrier's MQTT v5 dialect row: the live `MqttClient`, its connect/publish/subscribe surface printing the folded tracing value into `properties.userProperties` across every v5 packet, and the pub/sub event frames the boundary lifts to owned `Stream`s. Carrier folds own the typed tracing value (`interchange/carrier`); this catalog owns the client and its `userProperties` transport.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `mqtt`
- package: `mqtt` (MIT)
- module: single barrel `build/index.js` (CJS) + `build/index.d.ts`; `exports` routes `react-native`/`browser` to the ESM/min bundle, no deep subpath
- runtime: isomorphic — node/bun carry the TCP/TLS builders and stream deps, browser ships the WebSocket-only bundle; `connect` resolves the transport by URL scheme (`mqtt`/`mqtts`/`ws`/`wss`/`tcp`/`ssl`)
- rail: interchange/carrier

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the live client, its per-operation option frames whose `properties.userProperties` is the carrier's MQTT dialect target, and the v5 property and grade brands

| [INDEX] | [SYMBOL]                                        | [TYPE_FAMILY]   | [CAPABILITY]                                                       |
| :-----: | :---------------------------------------------- | :-------------- | :----------------------------------------------------------------- |
|  [01]   | `MqttClient` (`Client`)                         | live client     | `TypedEventEmitter<MqttClientEventCallbacks>`; resource not value  |
|  [02]   | `IClientOptions`                                | connect frame   | `protocolVersion` 5, CONNECT `userProperties`, `will`, `keepalive` |
|  [03]   | `IClientPublishOptions`                         | publish frame   | `qos`/`retain`/`dup`; PUBLISH `userProperties`, `contentType`      |
|  [04]   | `IClientSubscribeOptions`                       | subscribe frame | `qos`/`nl`/`rap`/`rh`; SUBSCRIBE `userProperties`                  |
|  [05]   | `IClientUnsubscribeProperties`                  | unsub frame     | UNSUBSCRIBE `userProperties`                                       |
|  [06]   | `ISubscriptionMap` / `ISubscriptionGrant`       | topic map       | `{[topic]: IClientSubscribeOptions}`; `qos: QoS \| 128` refuses    |
|  [07]   | `MqttClientEventCallbacks`                      | event map       | typed `connect`/`message`/`disconnect`/`error`/`close`/`offline`   |
|  [08]   | `UserProperties` (`mqtt-packet`)                | v5 prop map     | `{[key]: string \| string[]}` — carrier `userProperties` target    |
|  [09]   | `QoS` (`mqtt-packet`)                           | delivery grade  | `0 \| 1 \| 2`; QoS 1/2 route through the per-client `IStore`       |
|  [10]   | `MqttProtocol` / `BaseMqttProtocol`             | scheme brand    | `mqtt`/`mqtts`/`ws`/`wss`/`tcp`/`ssl` (+`+unix`) selector          |
|  [11]   | `ErrorWithReasonCode` / `ErrorWithSubackPacket` | typed fault     | `code: number` / `packet: ISubackPacket` → `FaultClass`            |
|  [12]   | `IStore` / `Store`                              | QoS store       | per-client `put`/`get`/`del`/`createStream`; never global          |
|  [13]   | `TimerVariant` (`auto`/`worker`/`native`)       | keepalive clock | `worker` = `worker-timers` throttle-immune browser keepalive       |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: client acquisition, publish/subscribe carrying `userProperties`, event-frame consumption, and scoped release

| [INDEX] | [SURFACE]                                        | [SHAPE]  | [CAPABILITY]                                               |
| :-----: | :----------------------------------------------- | :------- | :--------------------------------------------------------- |
|  [01]   | `connect(brokerUrl \| opts): MqttClient`         | factory  | live client; `protocolVersion: 5` selects the v5 surface   |
|  [02]   | `connectAsync`                                   | factory  | first CONNACK; scoped acquire arm                          |
|  [03]   | `client.publishAsync(topic, message, opts?)`     | instance | carrier-printed `userProperties` on `opts.properties`      |
|  [04]   | `client.subscribeAsync`                          | instance | `Promise<ISubscriptionGrant[]>`; `qos: 128` grant refuses  |
|  [05]   | `client.unsubscribeAsync(topic, opts?)`          | instance | `userProperties` on the UNSUBSCRIBE frame                  |
|  [06]   | `client.on(event, cb)` (`TypedEventEmitter`)     | instance | `message`/`connect`/`disconnect`/`error`/`close`/`offline` |
|  [07]   | `client.handleMessage(...)` / `customHandleAcks` | instance | QoS 1/2 app-controlled ack with a v5 reason code           |
|  [08]   | `client.handleAuth(...)` / `authPacket`          | instance | enhanced-authentication AUTH (`authenticationMethod`)      |
|  [09]   | `client.endAsync(force?, opts?)`                 | instance | `opts` DISCONNECT carries `userProperties`; release arm    |
|  [10]   | `client.reconnect(opts?)`                        | instance | manual reconnect reusing `incomingStore`/`outgoingStore`   |
|  [11]   | `ReasonCodes`                                    | static   | `{ [code: number]: string }` — the v5 reason-code map      |
|  [12]   | `client.connected` / `reconnecting`              | property | lifecycle read; the `Stream` scope reads these on close    |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `userProperties` is the v5 dialect target, never a header list: every CONNECT/PUBLISH/SUBSCRIBE/UNSUBSCRIBE/DISCONNECT packet carries application key/value pairs as `UserProperties` on its `properties` block, so the carrier's `mqtt` row prints into `opts.properties.userProperties` and reads them back off the delivered `IPublishPacket['properties'].userProperties`.
- client is a resource, not a value: `connect`/`connectAsync` return a live `MqttClient` holding sockets, keepalive timers, and per-client QoS stores; boundary composes it through `Effect.acquireRelease`, and each app owns its connection, subscription set, and message-id space.
- events lift per subscription, never a shared listener: each `subscribe` scopes its `message` frames into an owned `Stream`/`Queue` that `error`/`close`/`offline` terminate, and the `on` handler is that stream's internal pump.
- node-callback surface crosses at the seam: every verb ships a callback and an `*Async` overload; boundary consumes the `*Async` verbs under `Effect.tryPromise` and lifts emitter frames and callback-only paths (`customHandleAcks`, `handleAuth`) through `Effect.async`.

[STACKING]:
- `interchange/carrier` (`core/.planning/interchange/carrier.md`): `Carrier.inject('mqtt', context, frame)` prints the folded tracing value onto the `userProperties` record `publishAsync` ships, and `Carrier.extract('mqtt', frame)` reads it off the delivered packet's property block — this package realizes the carrier's `mqtt` dialect row.
- `cloudevents` (`core/.api/cloudevents.md`): `cloudevents` ships the `MQTT.binary`/`MQTT.toEvent` ENVELOPE codec, this ships the live client the envelope rides — a CloudEvents-over-MQTT lane serializes through the `cloudevents` binding then `publishAsync`es the frame, so the binding never opens a connection and the client never mints an envelope.
- `effect` `Stream` (`.api/effect.md`): `message` frames lift to a `Stream<IPublishPacket>` through `Stream.asyncScoped` registering the `on('message')` pump and tearing it down on scope close, so backpressure and interruption are owned.
- `effect` `Schema` (`.api/effect.md`): `Schema.decodeUnknown` decodes the `string | Buffer` payload once past the seam.
- `effect` `Match`/`Data` (`.api/effect.md`): the `MqttProtocol` scheme, `QoS` grade, and `ReasonCodes` verdict dispatch through `Match.exhaustive`, a transport scheme or reason code resolving as a table row over broker-URL strings.
- `value/fault` (`core/.planning/value/fault.md`): `ErrorWithReasonCode.code` and `ErrorWithSubackPacket.packet` project onto a `FaultClass` row at the `Effect.tryPromise` boundary through the `ReasonCodes` map.
- `value/identity` (`core/.planning/value/identity.md`): the carrier's `rasm.tenant` promotion rides one identity-branded baggage member the `mqtt` row prints into `userProperties`, so tenant context survives every broker hop.

[LOCAL_ADMISSION]:
- acquire through `Effect.acquireRelease(Effect.tryPromise(() => connectAsync(url, opts)), (c) => Effect.promise(() => c.endAsync()))` with `protocolVersion: 5`; each client stays inside its scope.
- route QoS 1/2 delivery through the per-client `Store` for at-least/exactly-once semantics; the in-memory default store is per-client and app-neutral.
- a `qos: 128` subscription grant is a refusal the boundary lifts onto `FaultClass`.

[RAIL_LAW]:
- Package: `mqtt`
- Owns: the live `MqttClient` and its `connect`/`publish`/`subscribe`/`unsubscribe`/`end`/`reconnect` surface, the `IClientOptions`/`IClientPublishOptions`/`IClientSubscribeOptions` frames whose `properties.userProperties` realizes the carrier's MQTT dialect row, the typed `MqttClientEventCallbacks` emitter, the per-client `IStore`/`Store` QoS surface, the `MqttProtocol` scheme brand, the `ReasonCodes` v5 vocabulary, and the `ErrorWithReasonCode`/`ErrorWithSubackPacket` faults
- Accept: `Effect.acquireRelease` acquisition on `connectAsync`/`endAsync`, tracing printed onto `opts.properties.userProperties` through the carrier folds, `message` frames scoped into a `Stream.asyncScoped`, `*Async` overloads under `Effect.tryPromise` and callback paths under `Effect.async`, `ErrorWithReasonCode`/`ErrorWithSubackPacket` mapped onto `FaultClass`, `MqttProtocol`/`ReasonCodes` dispatched as `Match` rows
- Reject: a process-global shared client, a bare `connect()` outliving its scope, a raw `client.on('message')` two apps register, a raw `userProperties` read bypassing the carrier folds, a callback where the `*Async` overload exists, a shared QoS `Store` across clients, a `qos: 128` refusal swallowed as success, the bundled `commist`/`help-me` CLI surface
